using ShopApi.Dtos.Favorites;

namespace ShopApi.Interfaces;

public interface IFavoriteService
{
    Task<List<FavoriteProductDto>> GetMyFavoritesAsync(
        int userId
    );

    Task AddAsync(
        int userId,
        int productId
    );

    Task RemoveAsync(
        int userId,
        int productId
    );
}