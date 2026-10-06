using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Cart;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(
        ICartService cartService
    )
    {
        _cartService =
            cartService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId =
            GetCurrentUserId();

        var result =
            await _cartService
                .GetMyCartAsync(
                    userId
                );

        return Ok(
            ApiResponse<CartDto>.Success(
                result,
                200,
                "Sepet getirildi."
            )
        );
    }

    [HttpPost("items/{productId:int}")]
    public async Task<IActionResult> AddItem(
        int productId,
        AddToCartDto dto
    )
    {
        var userId =
            GetCurrentUserId();

        var result =
            await _cartService.AddItemAsync(
                userId,
                productId,
                dto
            );

        return Ok(
            ApiResponse<CartDto>.Success(
                result,
                200,
                "Ürün sepete eklendi."
            )
        );
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(
        int cartItemId,
        UpdateCartItemDto dto
    )
    {
        var userId =
            GetCurrentUserId();

        var result =
            await _cartService.UpdateItemAsync(
                userId,
                cartItemId,
                dto
            );

        return Ok(
            ApiResponse<CartDto>.Success(
                result,
                200,
                "Sepet güncellendi."
            )
        );
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(
        int cartItemId
    )
    {
        var userId =
            GetCurrentUserId();

        var result =
            await _cartService.RemoveItemAsync(
                userId,
                cartItemId
            );

        return Ok(
            ApiResponse<CartDto>.Success(
                result,
                200,
                "Ürün sepetten çıkarıldı."
            )
        );
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var userId =
            GetCurrentUserId();

        await _cartService.ClearAsync(
            userId
        );

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Sepet temizlendi."
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