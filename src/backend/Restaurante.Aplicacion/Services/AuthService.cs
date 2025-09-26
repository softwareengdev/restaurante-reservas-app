using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Restaturante.Infraestructura.Repository;
using Restaurante.Aplicacion.Repository;
using Restaurante.Modelo.Model;
using Restaurante.Modelo.Model.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Restaurante.Aplicacion.Services
{


    public class AuthService : IAuthService
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        public readonly IRefreshTokenService _refreshTokenService;

        public AuthService(
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptionsSnapshot<JwtSettings> jwtSettings,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _refreshTokenService = refreshTokenService;

            if (string.IsNullOrEmpty(_jwtSettings.Key) || _jwtSettings.Key.Length < 32)
            {
                throw new InvalidOperationException("JWT Key is invalid: missing or too short (must be >324 chars for HS512). Check appsettings.json.");
            }
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Advanced: Check lockout, two-factor, etc.
            var signInResult = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: true);
            if (!signInResult.Succeeded)
            {
                if (signInResult.IsLockedOut)
                    throw new Exception("Account locked out");
                throw new UnauthorizedAccessException("Login failed");
            }

            return await GenerateTokensAsync(user);
        }

        public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FullName = request.FullName
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            return await GenerateTokensAsync(user);
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var refreshToken = await _refreshTokenService.ValidateAndGetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null || refreshToken.IsExpired || refreshToken.IsRevoked)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            // Revoke old refresh token and generate new ones
            await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken.Token);
            return await GenerateTokensAsync(user);
        }

        private async Task<TokenResponse> GenerateTokensAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("FullName", user.FullName ?? string.Empty)
            };

            // Add roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Advanced: Custom claims, audience per user, etc.

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); // Advanced: Stronger algorithm

            var accessToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes), // e.g., 15 mins
                signingCredentials: creds
            );

            var refreshTokenValue = GenerateRefreshToken();
            await _refreshTokenService.StoreRefreshTokenAsync(user.Id, refreshTokenValue, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)); // e.g., 7 days

            return new TokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = refreshTokenValue,
                ExpiresAt = accessToken.ValidTo
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public Task<TokenResponse> RevokeRefreshTokenAsync(string refreshToken)
        {
            _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);
            return (Task<TokenResponse>)Task.CompletedTask;
        }
    }
}
