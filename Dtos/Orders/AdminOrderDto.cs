namespace ShopApi.Dtos.Orders;

public class AdminOrderDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string CustomerName { get; set; } = "";

    public string CustomerEmail { get; set; } = "";

    public string Status { get; set; } = "";

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public int TotalQuantity { get; set; }
}