using CableWinding.Api.Models;
using CableWinding.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CableWinding.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Вход в систему (логин + пароль)</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest? req)
    {
        if (req == null)
            return BadRequest(new { error = "Тело запроса пусто. Отправьте JSON: { \"login\": \"...\", \"password\": \"...\" }" });
        if (string.IsNullOrWhiteSpace(req.Login) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Логин и пароль обязательны" });

        try
        {
            var result = _auth.TryLogin(req.Login, req.Password);
            if (result == null)
                return Unauthorized(new { error = "Неверный логин или пароль" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ошибка сервера: " + ex.Message });
        }
    }

    /// <summary>Проверка токена, возвращает данные текущего пользователя</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var name = User.Identity?.Name;
        var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        var displayName = User.Claims.FirstOrDefault(c => c.Type == "displayName")?.Value;
        return Ok(new { login = name, role, displayName });
    }
}
