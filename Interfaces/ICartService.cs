using ShopApi.Dtos.Cart;

namespace ShopApi.Interfaces;

public interface ICartService
{
    Task<CartDto> GetMyCartAsync(
        int userId
    );

    Task<CartDto> AddItemAsync(
        int userId,
        int productId,
        AddToCartDto dto
    );

    Task<CartDto> UpdateItemAsync(
        int userId,
        int cartItemId,
        UpdateCartItemDto dto
    );

    Task<CartDto> RemoveItemAsync(
        int userId,
        int cartItemId
    );

    Task ClearAsync(
        int userId
    );
}