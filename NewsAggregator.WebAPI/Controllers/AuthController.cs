using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregator.Core.DTOs;
using NewsAggregator.Core.Interfaces;
using System.Security.Claims;

namespace NewsAggregator.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var (result, error) = await _auth.RegisterAsync(dto);
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (result, error) = await _auth.LoginAsync(dto);
        if (error != null) return Unauthorized(new { message = error });
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _auth.RefreshTokenAsync(dto.RefreshToken);
        if (result == null) return Unauthorized(new { message = "Недействительный токен" });
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _auth.ForgotPasswordAsync(dto.Email);
        // Всегда возвращаем Ok (не раскрываем существование email)
        return Ok(new { message = "Если email существует — письмо отправлено" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var success = await _auth.ResetPasswordAsync(dto);
        if (!success) return BadRequest(new { message = "Недействительный или истёкший токен" });
        return Ok(new { message = "Пароль успешно изменён" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user   = await _auth.GetCurrentUserAsync(userId);
        if (user == null) return NotFound();
        return Ok(user);
    }
}