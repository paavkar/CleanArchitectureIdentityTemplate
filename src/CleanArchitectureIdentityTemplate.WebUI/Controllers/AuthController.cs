using Asp.Versioning;
using CleanArchitectureIdentityTemplate.Application.Auth;
using CleanArchitectureIdentityTemplate.Application.DTOs;
using CleanArchitectureIdentityTemplate.Application.ResultModels;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureIdentityTemplate.WebUI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            AuthResult result = await authService.RegisterAsync(registerDto);
            return !result.Succeeded ? BadRequest(result) : Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            AuthResult result = await authService.LoginAsync(loginDto);
            return !result.Succeeded ? Unauthorized(result) : Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            AuthResult result = await authService.RefreshTokenAsync(refreshToken);
            return !result.Succeeded ? BadRequest(result) : Ok(result);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            var success = await authService.RevokeRefreshTokenAsync(refreshToken);
            return !success ? BadRequest(new { Message = "Invalid refresh token." }) : Ok(new { Message = "Refresh token revoked." });
        }
    }
}
