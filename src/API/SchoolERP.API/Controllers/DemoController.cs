using Microsoft.AspNetCore.Mvc;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.API.Controllers;

public class DemoController : BaseApiController
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("Pong", "Service is healthy!");
    }

    // ✅ Method ka naam "TestError" rakha hai taaki "Error" type se collide na ho
    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        var error = Error.NotFound("Student", "101");
        return Fail(error); // 👈 "Fail" method use kar rahe hain
    }
}