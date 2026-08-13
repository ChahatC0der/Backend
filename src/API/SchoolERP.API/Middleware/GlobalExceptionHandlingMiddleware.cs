using System.Net;
using System.Text.Json;
using FluentValidation;

namespace SchoolERP.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

        var (statusCode, message) = ex switch
        {
            ValidationException validationEx =>
                (HttpStatusCode.BadRequest, string.Join(" | ", validationEx.Errors.Select(e => e.ErrorMessage))),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "You are not authenticated."),

            KeyNotFoundException =>
                (HttpStatusCode.NotFound, ex.Message),

            _ => (HttpStatusCode.InternalServerError,
                  _env.IsDevelopment() ? ex.Message : "An unexpected error occurred. Please try again later.")
        };

        var response = new
        {
            success = false,
            error = message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}