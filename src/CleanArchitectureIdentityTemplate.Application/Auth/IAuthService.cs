using CleanArchitectureIdentityTemplate.Application.DTOs;
using CleanArchitectureIdentityTemplate.Application.ResultModels;

namespace CleanArchitectureIdentityTemplate.Application.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto registerDto, string platform);
        Task<AuthResult> LoginAsync(LoginDto loginDto, string platform);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);

        Task<AuthResult> VerifyTwoFactorAsync(TwoFactorDto twoFactorDto, string platform);
        Task<AuthResult> SetupTwoFactorAsync(string userId);
        Task<AuthResult> EnableTwoFactorAsync(string userId, VerifyTwoFactorDto model);
        Task<AuthResult> DisableTwoFactorAsync(string userId);
    }
}
