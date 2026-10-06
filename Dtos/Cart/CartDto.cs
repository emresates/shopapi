namespace ShopApi.Dtos.Cart;

public class CartDto
{
    public int Id { get; set; }

    public List<CartItemDto> Items { get; set; } = new();

    public int TotalQuantity { get; set; }

    public decimal TotalPrice { get; set; }
}