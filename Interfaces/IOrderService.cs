using ShopApi.Dtos.Orders;

namespace ShopApi.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(
        int userId,
        CreateOrderDto dto
    );

    Task<List<OrderDto>> GetMyOrdersAsync(
        int userId
    );

    Task<OrderDto> GetByIdAsync(
        int userId,
        int orderId
    );

    Task<OrderDto> UpdateStatusAsync(
        int orderId,
        UpdateOrderStatusDto dto,
        int adminUserId
    );

    Task<OrderDto> CancelAsync(
    int userId,
    int orderId
);

    Task<List<AdminOrderDto>> GetAllForAdminAsync();

    Task<List<OrderStatusHistoryDto>> GetStatusHistoryAsync(
        int orderId,
        int requestingUserId,
        bool isAdmin
    );
}