using ShopApi.Constants;

namespace ShopApi.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Status { get; set; } =
    OrderStatuses.Pending;

    public decimal TotalPrice { get; set; }

    public string ShippingFullName { get; set; } = "";

    public string ShippingPhone { get; set; } = "";

    public string ShippingCity { get; set; } = "";

    public string ShippingDistrict { get; set; } = "";

    public string ShippingAddressLine { get; set; } = "";

    public string? ShippingPostalCode { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();

    public List<OrderStatusHistory> StatusHistory
    { get; set; } = new();
}