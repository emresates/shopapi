namespace ShopApi.Dtos.Favorites;

public class FavoriteProductDto
{
    public int ProductId { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = "";

    public string? MainImageUrl { get; set; }

    public DateTime FavoritedAt { get; set; }
}