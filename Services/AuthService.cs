using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopApi.Constants;
using ShopApi.Data;
using ShopApi.Dtos.Auth;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;

namespace ShopApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(
        AppDbContext context,
        ITokenService tokenService
    )
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto
    )
    {
        var email =
            dto.Email
                .Trim()
                .ToLowerInvariant();

        var emailExists =
            await _context.Users
                .AnyAsync(
                    x => x.Email == email
                );

        if (emailExists)
        {
            throw new AppException(
                "Bu email adresi zaten kullanılıyor.",
                409,
                "emailAlreadyExists"
            );
        }

        var user = new User
        {
            Name = dto.Name.Trim(),

            Email = email,

            Role = Roles.Customer,

            IsActive = true
        };

        var passwordHasher =
            new PasswordHasher<User>();

        user.PasswordHash =
            passwordHasher.HashPassword(
                user,
                dto.Password
            );

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var accessToken =
            _tokenService.CreateAccessToken(
                user
            );

        return new AuthResponseDto
        {
            AccessToken = accessToken
        };
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto
    )
    {
        var email =
            dto.Email
                .Trim()
                .ToLowerInvariant();

        var user =
            await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Email == email
                );

        if (user == null)
        {
            throw new AppException(
                "Email veya şifre hatalı.",
                401,
                "invalidCredentials"
            );
        }

        if (!user.IsActive)
        {
            throw new AppException(
                "Hesabınız devre dışı bırakılmış.",
                403,
                "userDisabled"
            );
        }

        var passwordHasher =
            new PasswordHasher<User>();

        var passwordResult =
            passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password
            );

        if (
            passwordResult ==
            PasswordVerificationResult.Failed
        )
        {
            throw new AppException(
                "Email veya şifre hatalı.",
                401,
                "invalidCredentials"
            );
        }

        var accessToken =
            _tokenService.CreateAccessToken(
                user
            );

        return new AuthResponseDto
        {
            AccessToken = accessToken
        };
    }
}