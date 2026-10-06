namespace ShopApi.Dtos.Orders;

public class OrderDto
{
    public int Id { get; set; }

    public string Status { get; set; } = "";

    public decimal TotalPrice { get; set; }

    public string ShippingFullName { get; set; } = "";

    public string ShippingCity { get; set; } = "";

    public string ShippingDistrict { get; set; } = "";

    public string ShippingAddressLine { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
}