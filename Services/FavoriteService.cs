using Microsoft.EntityFrameworkCore;
using ShopApi.Data;
using ShopApi.Dtos.Favorites;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;

namespace ShopApi.Services;

public class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _context;

    public FavoriteService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<List<FavoriteProductDto>>
        GetMyFavoritesAsync(
            int userId
        )
    {
        return await _context.Favorites
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new FavoriteProductDto
            {
                ProductId =
                    x.ProductId,

                Name =
                    x.Product.Name,

                Description =
                    x.Product.Description,

                Price =
                    x.Product.Price,

                Stock =
                    x.Product.Stock,

                CategoryId =
                    x.Product.CategoryId,

                CategoryName =
                    x.Product.Category.Name,

                MainImageUrl =
                    x.Product.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                FavoritedAt =
                    x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task AddAsync(
        int userId,
        int productId
    )
    {
        var productExists =
            await _context.Products
                .AnyAsync(
                    x => x.Id == productId
                );

        if (!productExists)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        var alreadyFavorite =
            await _context.Favorites
                .AnyAsync(
                    x =>
                        x.UserId == userId &&
                        x.ProductId == productId
                );

        if (alreadyFavorite)
        {
            throw new AppException(
                "Ürün zaten favorilerinizde.",
                409,
                "productAlreadyFavorite"
            );
        }

        var favorite =
            new Favorite
            {
                UserId = userId,
                ProductId = productId
            };

        _context.Favorites.Add(
            favorite
        );

        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(
        int userId,
        int productId
    )
    {
        var favorite =
            await _context.Favorites
                .FirstOrDefaultAsync(
                    x =>
                        x.UserId == userId &&
                        x.ProductId == productId
                );

        if (favorite == null)
        {
            throw new AppException(
                "Ürün favorilerinizde bulunamadı.",
                404,
                "favoriteNotFound"
            );
        }

        _context.Favorites.Remove(
            favorite
        );

        await _context.SaveChangesAsync();
    }
}