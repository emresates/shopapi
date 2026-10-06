using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ShopApi.Interfaces;
using ShopApi.Models;

namespace ShopApi.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(
        IConfiguration configuration
    )
    {
        _configuration = configuration;
    }

    public string CreateAccessToken(
        User user
    )
    {
        var jwtKey =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key bulunamadı."
            );

        var issuer =
            _configuration["Jwt:Issuer"];

        var audience =
            _configuration["Jwt:Audience"];

        var expireMinutes =
            int.TryParse(
                _configuration["Jwt:ExpireMinutes"],
                out var minutes
            )
                ? minutes
                : 15;

        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new(
                    ClaimTypes.Name,
                    user.Name
                ),

                new(
                    ClaimTypes.Email,
                    user.Email
                ),

                new(
                    ClaimTypes.Role,
                    user.Role
                )
            };

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    jwtKey
                )
            );

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires:
                    DateTime.UtcNow.AddMinutes(
                        expireMinutes
                    ),
                signingCredentials:
                    credentials
            );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}