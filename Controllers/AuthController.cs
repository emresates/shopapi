using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Auth;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService
    )
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterDto dto
    )
    {
        var result =
            await _authService
                .RegisterAsync(dto);

        return StatusCode(
            201,
            ApiResponse<AuthResponseDto>
                .Success(
                    result,
                    201,
                    "Kullanıcı oluşturuldu."
                )
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginDto dto
    )
    {
        var result =
            await _authService
                .LoginAsync(dto);

        return Ok(
            ApiResponse<AuthResponseDto>
                .Success(
                    result,
                    200,
                    "Giriş başarılı."
                )
        );
    }
}