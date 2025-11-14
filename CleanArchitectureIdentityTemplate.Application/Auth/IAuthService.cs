using CleanArchitectureIdentityTemplate.Application.DTOs;
using CleanArchitectureIdentityTemplate.Application.ResultModels;

namespace CleanArchitectureIdentityTemplate.Application.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto registerDto);
        Task<AuthResult> LoginAsync(LoginDto loginDto);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
