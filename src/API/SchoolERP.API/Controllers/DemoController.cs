using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;

namespace SchoolERP.API.Controllers;

public class DemoController : BaseApiController
{
    [HttpGet("ping")]
    [AllowAnonymous] // Login ke bina access
    public IActionResult Ping() => Ok("Pong", "Service is healthy!");

    [HttpGet("admin-only")]
    [HasPermission("student.read")] // 👈 SIRF ADMIN ke paas ye permission hai
    public IActionResult AdminOnly() => Ok("You have student.read permission!");
}