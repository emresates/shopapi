namespace ShopApi.Dtos.Orders;

public class OrderStatusHistoryDto
{
    public int Id { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = "";

    public int? ChangedByUserId { get; set; }

    public string? ChangedByName { get; set; }

    public DateTime ChangedAt { get; set; }
}