namespace ShopApi.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public List<ProductImage> Images { get; set; } = new();

    public List<Favorite> Favorites { get; set; } = new();

    public List<CartItem> CartItems { get; set; } = new();

    public List<OrderItem> OrderItems { get; set; } = new();
}