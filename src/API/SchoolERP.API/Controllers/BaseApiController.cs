using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    // ✅ SUCCESS: Standardized 200 OK with data + optional message
    protected IActionResult Ok<T>(T data, string? message = null)
    {
        if (!string.IsNullOrEmpty(message))
            return base.Ok(new { success = true, message, data });
        return base.Ok(new { success = true, data });
    }

    // ✅ FAILURE: Centralized error handler (⚠️ Renamed from "Error" to "Fail" to avoid collision with Error class)
    protected IActionResult Fail(Error error)
    {
        var response = new { success = false, error = error.Message };

        // 🔥 Status Code Mapping (404, 400, 401, 409, 500)
        return error.Code switch
        {
            "NotFound" => NotFound(response),
            "Validation" => BadRequest(response),
            "Unauthorized" => Unauthorized(response),
            "Conflict" => Conflict(response),
            _ => StatusCode(500, new { success = false, error = "An unexpected error occurred." })
        };
    }

    // 🎯 MASTER HANDLER: Maps Result<T> to IActionResult (Automatically picks 200 or Fail)
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value, result.Message);

        return Fail(result.Error);   // 👈 Naya naam yahan use ho raha hai
    }
}