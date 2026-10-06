using Microsoft.EntityFrameworkCore;
using ShopApi.Data;
using ShopApi.Dtos.Cart;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;

namespace ShopApi.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<CartDto> GetMyCartAsync(
        int userId
    )
    {
        var cart =
            await GetOrCreateCartAsync(
                userId
            );

        return await BuildCartDtoAsync(
            cart.Id
        );
    }

    public async Task<CartDto> AddItemAsync(
        int userId,
        int productId,
        AddToCartDto dto
    )
    {
        var product =
            await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Id == productId
                );

        if (product == null)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        if (product.Stock <= 0)
        {
            throw new AppException(
                "Ürün stokta bulunmuyor.",
                409,
                "productOutOfStock"
            );
        }

        var cart =
            await GetOrCreateCartAsync(
                userId
            );

        var existingItem =
            await _context.CartItems
                .FirstOrDefaultAsync(
                    x =>
                        x.CartId == cart.Id &&
                        x.ProductId == productId
                );

        if (existingItem != null)
        {
            var newQuantity =
                existingItem.Quantity +
                dto.Quantity;

            if (newQuantity > product.Stock)
            {
                throw new AppException(
                    "Sepet miktarı mevcut stoktan fazla olamaz.",
                    409,
                    "insufficientStock"
                );
            }

            existingItem.Quantity =
                newQuantity;
        }
        else
        {
            if (dto.Quantity > product.Stock)
            {
                throw new AppException(
                    "Sepet miktarı mevcut stoktan fazla olamaz.",
                    409,
                    "insufficientStock"
                );
            }

            var cartItem =
                new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = dto.Quantity
                };

            _context.CartItems.Add(
                cartItem
            );
        }

        await _context.SaveChangesAsync();

        return await BuildCartDtoAsync(
            cart.Id
        );
    }

    public async Task<CartDto> UpdateItemAsync(
        int userId,
        int cartItemId,
        UpdateCartItemDto dto
    )
    {
        var cart =
            await GetOrCreateCartAsync(
                userId
            );

        var cartItem =
            await _context.CartItems
                .Include(x => x.Product)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == cartItemId &&
                        x.CartId == cart.Id
                );

        if (cartItem == null)
        {
            throw new AppException(
                "Sepet ürünü bulunamadı.",
                404,
                "cartItemNotFound"
            );
        }

        if (
            dto.Quantity >
            cartItem.Product.Stock
        )
        {
            throw new AppException(
                "Sepet miktarı mevcut stoktan fazla olamaz.",
                409,
                "insufficientStock"
            );
        }

        cartItem.Quantity =
            dto.Quantity;

        await _context.SaveChangesAsync();

        return await BuildCartDtoAsync(
            cart.Id
        );
    }

    public async Task<CartDto> RemoveItemAsync(
        int userId,
        int cartItemId
    )
    {
        var cart =
            await GetOrCreateCartAsync(
                userId
            );

        var cartItem =
            await _context.CartItems
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == cartItemId &&
                        x.CartId == cart.Id
                );

        if (cartItem == null)
        {
            throw new AppException(
                "Sepet ürünü bulunamadı.",
                404,
                "cartItemNotFound"
            );
        }

        _context.CartItems.Remove(
            cartItem
        );

        await _context.SaveChangesAsync();

        return await BuildCartDtoAsync(
            cart.Id
        );
    }

    public async Task ClearAsync(
        int userId
    )
    {
        var cart =
            await GetOrCreateCartAsync(
                userId
            );

        await _context.CartItems
            .Where(
                x => x.CartId == cart.Id
            )
            .ExecuteDeleteAsync();
    }

    private async Task<Cart>
        GetOrCreateCartAsync(
            int userId
        )
    {
        var cart =
            await _context.Carts
                .FirstOrDefaultAsync(
                    x => x.UserId == userId
                );

        if (cart != null)
        {
            return cart;
        }

        cart = new Cart
        {
            UserId = userId
        };

        _context.Carts.Add(cart);

        await _context.SaveChangesAsync();

        return cart;
    }

    private async Task<CartDto>
        BuildCartDtoAsync(
            int cartId
        )
    {
        var items =
            await _context.CartItems
                .AsNoTracking()
                .Where(
                    x => x.CartId == cartId
                )
                .OrderBy(x => x.Id)
                .Select(
                    x => new CartItemDto
                    {
                        Id = x.Id,

                        ProductId =
                            x.ProductId,

                        ProductName =
                            x.Product.Name,

                        MainImageUrl =
                            x.Product.Images
                                .Where(
                                    i => i.IsMain
                                )
                                .Select(
                                    i => i.ImageUrl
                                )
                                .FirstOrDefault(),

                        UnitPrice =
                            x.Product.Price,

                        Quantity =
                            x.Quantity,

                        LineTotal =
                            x.Product.Price *
                            x.Quantity,

                        Stock =
                            x.Product.Stock
                    }
                )
                .ToListAsync();

        return new CartDto
        {
            Id = cartId,

            Items = items,

            TotalQuantity =
                items.Sum(
                    x => x.Quantity
                ),

            TotalPrice =
                items.Sum(
                    x => x.LineTotal
                )
        };
    }
}