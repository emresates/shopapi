using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Favorites;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(
        IFavoriteService favoriteService
    )
    {
        _favoriteService =
            favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId =
            GetCurrentUserId();

        var result =
            await _favoriteService
                .GetMyFavoritesAsync(
                    userId
                );

        return Ok(
            ApiResponse<List<FavoriteProductDto>>
                .Success(
                    result,
                    200,
                    "Favoriler getirildi."
                )
        );
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(
        int productId
    )
    {
        var userId =
            GetCurrentUserId();

        await _favoriteService.AddAsync(
            userId,
            productId
        );

        return StatusCode(
            201,
            ApiResponse<object>.Success(
                new { },
                201,
                "Ürün favorilere eklendi."
            )
        );
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(
        int productId
    )
    {
        var userId =
            GetCurrentUserId();

        await _favoriteService.RemoveAsync(
            userId,
            productId
        );

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Ürün favorilerden çıkarıldı."
            )
        );
    }

    private int GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (
            !int.TryParse(
                value,
                out var userId
            )
        )
        {
            throw new AppException(
                "Geçersiz kullanıcı.",
                401,
                "invalidToken"
            );
        }

        return userId;
    }
}