using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NewsAggregator.Core.DTOs;
using NewsAggregator.Core.Entities;
using NewsAggregator.Core.Interfaces;
using NewsAggregator.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NewsAggregator.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEmailService _email;

    public AuthService(AppDbContext db, IConfiguration config, IEmailService email)
    {
        _db     = db;
        _config = config;
        _email  = email;
    }

    public async Task<(AuthResponseDto? Result, string? Error)> RegisterAsync(RegisterDto dto)
    {
        // Проверка уникальности email
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return (null, "Email уже используется");

        // Проверка уникальности username
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return (null, "Имя пользователя уже занято");

        // Проверяем — есть ли уже Admin в системе
        var adminExists = await _db.Users.AnyAsync(u => u.Role == "Admin");

        var user = new User
        {
            Username     = dto.Username,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            // Первый зарегистрированный — Admin, остальные — User
            Role         = adminExists ? "User" : "Admin",
            CreatedAt    = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (await GenerateTokensAsync(user), null);
    }

    public async Task<(AuthResponseDto? Result, string? Error)> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return (null, "Неверный email или пароль");

        return (await GenerateTokensAsync(user), null);
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false; // Не сообщаем что email не найден (безопасность)

        // Удаляем старые токены
        var oldTokens = _db.PasswordResetTokens.Where(t => t.UserId == user.Id);
        _db.PasswordResetTokens.RemoveRange(oldTokens);

        var token = new PasswordResetToken
        {
            UserId    = user.Id,
            Token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync();

        // Отправляем письмо
        var resetLink = $"{_config["AppUrl"]}/reset-password?token={Uri.EscapeDataString(token.Token)}";
        await _email.SendPasswordResetAsync(user.Email, user.Username, resetLink);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var tokenRecord = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == dto.Token &&
                !t.IsUsed &&
                t.ExpiresAt > DateTimeOffset.UtcNow);

        if (tokenRecord == null) return false;

        tokenRecord.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        tokenRecord.IsUsed = true;
        await _db.SaveChangesAsync();

        return true;
    }
    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken &&
            u.RefreshTokenExpiry > DateTimeOffset.UtcNow);

        if (user == null) return null;
        return await GenerateTokensAsync(user);
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return null;

        return new UserInfoDto
        {
            Id        = user.Id,
            Username  = user.Username,
            Email     = user.Email,
            Role      = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<AuthResponseDto> GenerateTokensAsync(User user)
    {
        var accessToken  = GenerateJwtToken(user);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);
        await _db.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken  = accessToken,
            RefreshToken = refreshToken,
            Username     = user.Username,
            Role         = user.Role
        };
    }

    private string GenerateJwtToken(User user)
    {
        var key   = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role)
        };

        var token = new JwtSecurityToken(
            issuer:             _config["JwtSettings:Issuer"],
            audience:           _config["JwtSettings:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}