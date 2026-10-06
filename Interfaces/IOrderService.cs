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
}