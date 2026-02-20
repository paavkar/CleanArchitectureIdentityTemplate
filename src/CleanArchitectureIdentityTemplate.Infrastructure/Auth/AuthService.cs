using CleanArchitectureIdentityTemplate.Application.Auth;
using CleanArchitectureIdentityTemplate.Application.DTOs;
using CleanArchitectureIdentityTemplate.Application.ResultModels;
using CleanArchitectureIdentityTemplate.Domain.Users;
using CleanArchitectureIdentityTemplate.SharedKernel.Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace CleanArchitectureIdentityTemplate.Infrastructure.Auth
{
    public class AuthService(
        HybridCache cache,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        RoleManager<IdentityRole> roleManager,
        IStringLocalizer<AppLocalization> localizer,
        IOptions<IdentityOptions> identityOptions) : IAuthService
    {
        private const int RefreshTokenExpiryDays = 7;

        public async Task<AuthResult> RegisterAsync(RegisterDto registerDto, string platform)
        {
            ApplicationUser user = new()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
            };

            IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(e =>
                    e.Code switch
                    {
                        "DuplicateEmail" => localizer[e.Code, registerDto.Email].ToString(),
                        "DuplicateUserName" => localizer[e.Code, registerDto.UserName].ToString(),
                        "PasswordTooShort" => localizer[e.Code, identityOptions.Value.Password.RequiredLength].ToString(),
                        _ => localizer[e.Code].ToString()
                    })
                };
            }

            var userRoleExists = await roleManager.RoleExistsAsync("User");
            if (!userRoleExists)
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            await userManager.AddToRoleAsync(user, "User");

            platform = string.IsNullOrWhiteSpace(platform) ? "default" : platform;
            return await GenerateAuthResultAsync(user, platform);
        }

        public async Task<AuthResult> LoginAsync(LoginDto loginDto, string platform)
        {
            ApplicationUser? user = string.IsNullOrWhiteSpace(loginDto.UserName)
                ? await userManager.FindByEmailAsync(loginDto.Email)
                : await userManager.FindByNameAsync(loginDto.UserName);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["InvalidLogin"]]
                };
            }

            if (user.TwoFactorEnabled)
            {
                return new AuthResult
                {
                    Succeeded = true,
                    TwoFactorRequired = true
                };
            }

            platform = string.IsNullOrWhiteSpace(platform) ? "default" : platform;
            return await GenerateAuthResultAsync(user, platform);
        }

        public async Task<AuthResult> VerifyTwoFactorAsync(TwoFactorDto twoFactorDto, string platform)
        {
            ApplicationUser? user = string.IsNullOrWhiteSpace(twoFactorDto.UserName)
                ? await userManager.FindByEmailAsync(twoFactorDto.Email)
                : await userManager.FindByNameAsync(twoFactorDto.UserName);

            if (user is null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["UserNotFound"]]
                };
            }

            var isValid = await userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultAuthenticatorProvider,
                twoFactorDto.Code);

            return !isValid
                ? new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["Invalid2FACode"]]
                }
                : await GenerateAuthResultAsync(user, platform);
        }

        public async Task<AuthResult> SetupTwoFactorAsync(string userId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["UserNotFound"]]
                };
            }

            var key = await userManager.GetAuthenticatorKeyAsync(user);
            if (key is null)
            {
                IdentityResult updated = await userManager.ResetAuthenticatorKeyAsync(user);
                if (!updated.Succeeded)
                {
                    return new AuthResult
                    {
                        Succeeded = false,
                        Errors = [localizer["2FAKeyGenerationFailed"]]
                    };
                }
                key = await userManager.GetAuthenticatorKeyAsync(user);
            }

            var issuer = configuration["Jwt:Issuer"];
            var authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
            var otpauthUri = string.Format(
                CultureInfo.InvariantCulture,
                authenticatorUriFormat,
                UrlEncoder.Default.Encode(issuer),
                UrlEncoder.Default.Encode(user.Email),
                key);

            return new AuthResult
            {
                Succeeded = true,
                TwoFactorUri = otpauthUri
            };
        }

        public async Task<AuthResult> EnableTwoFactorAsync(string userId, VerifyTwoFactorDto model)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["UserNotFound"]]
                };
            }

            var isValid = await userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultAuthenticatorProvider,
                model.Code);

            if (!isValid)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["Invalid2FACode"]]
                };
            }

            IdentityResult result = await userManager.SetTwoFactorEnabledAsync(user, true);

            return !result.Succeeded
                ? new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["SetTwoFactorFailed"]]
                }
                : new AuthResult
                {
                    Succeeded = true
                };
        }

        public async Task<AuthResult> DisableTwoFactorAsync(string userId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["UserNotFound"]]
                };
            }

            if (!user.TwoFactorEnabled)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["2FANotEnabled"]]
                };
            }

            IdentityResult disableResult = await userManager.SetTwoFactorEnabledAsync(user, false);

            if (!disableResult.Succeeded)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["SetTwoFactorFailed"]]
                };
            }

            await userManager.ResetAuthenticatorKeyAsync(user);

            return new AuthResult
            {
                Succeeded = true
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            RefreshTokenInfo tokenInfo = await cache.GetOrCreateAsync<RefreshTokenInfo>(refreshToken, async entry => null);
            if (tokenInfo == null || tokenInfo.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["InvalidRefresh"]]
                };
            }

            ApplicationUser? user = await userManager.FindByIdAsync(tokenInfo.UserId);
            if (user == null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = [localizer["UserNotFound"]]
                };
            }

            // This is to ensure the old refresh token cannot be used
            await cache.RemoveAsync(refreshToken);
            return await GenerateAuthResultAsync(user, tokenInfo.Platform);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            await cache.RemoveAsync(refreshToken);
            return true;
        }

        private async Task<AuthResult> GenerateAuthResultAsync(ApplicationUser user, string platform)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]!);

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
                new(JwtRegisteredClaimNames.Email, user.Email!),
            ];

            IList<string> roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"]
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            var refreshToken = GenerateRefreshToken();
            RefreshTokenInfo refreshTokenInfo = new()
            {
                Id = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpiryDays),
                Platform = platform
            };
            await cache.SetAsync(refreshToken, refreshTokenInfo, new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(RefreshTokenExpiryDays)
            });

            return new AuthResult
            {
                Succeeded = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
