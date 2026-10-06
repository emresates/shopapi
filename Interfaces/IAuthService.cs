using ShopApi.Dtos.Auth;

namespace ShopApi.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto
    );

    Task<AuthResponseDto> LoginAsync(
        LoginDto dto
    );
}