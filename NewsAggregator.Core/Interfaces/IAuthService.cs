using NewsAggregator.Core.DTOs;

namespace NewsAggregator.Core.Interfaces;

public interface IAuthService
{
    Task<(AuthResponseDto? Result, string? Error)> RegisterAsync(RegisterDto dto);
    Task<(AuthResponseDto? Result, string? Error)> LoginAsync(LoginDto dto);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<UserInfoDto?> GetCurrentUserAsync(int userId);
}