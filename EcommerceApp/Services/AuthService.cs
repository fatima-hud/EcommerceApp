using EcommerceApp.Entities;
using EcommerceApp.Helper;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static EcommerceApp.Services.AuthService;

namespace EcommerceApp.Services
{
    public class AuthService : IAuthService
    {
        
        private readonly JwtSettings _jwt;

        public AuthService(IOptions<JwtSettings>jwt)
        {
            _jwt = jwt.Value;
        }

        public Task<string> GetTokenAsync(User user)
        { 
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("isAdmin", user.IsAdmin.ToString())
        };

                var token = new JwtSecurityToken(
                    issuer: _jwt.Issuer,
                    audience: _jwt.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiresInMinutes),
                    signingCredentials: creds
                );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult(tokenString);
        }
        }

    }

    


