using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace MSH.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly PasswordHasher<object> _passwordHasher;

    public TestController()
    {
        _passwordHasher = new PasswordHasher<object>();
    }

    [HttpGet("generate-hash")]
    public IActionResult GenerateHash([FromQuery] string password = "Admin123!")
    {
        var hash = _passwordHasher.HashPassword(null, password);
        return Ok(new { password, hash });
    }
} 