using System.Net;
using System.Text.Json;
using FluentValidation;

namespace SchoolERP.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

        // 🔥 Status Code Mapping based on Exception Type
        var (statusCode, title, detail) = ex switch
        {
            ValidationException validationEx =>
                (HttpStatusCode.BadRequest, "Validation Error", string.Join(" | ", validationEx.Errors.Select(e => e.ErrorMessage))),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized", "You are not authenticated."),

            KeyNotFoundException =>
                (HttpStatusCode.NotFound, "Resource Not Found", ex.Message),

            _ => (HttpStatusCode.InternalServerError, "Server Error", "An unexpected error occurred.")
        };

        var response = new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = title,
            status = (int)statusCode,
            detail = detail,
            instance = context.Request.Path,
            traceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}