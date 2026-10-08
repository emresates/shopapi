using Microsoft.EntityFrameworkCore;
using ShopApi.Constants;
using ShopApi.Data;
using ShopApi.Dtos.Orders;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;
using ShopApi.Rules;

namespace ShopApi.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<OrderDto> CreateAsync(
    int userId,
    CreateOrderDto dto
)
    {
        var address =
            await _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == dto.AddressId &&
                        x.UserId == userId
                );

        if (address == null)
        {
            throw new AppException(
                "Adres bulunamadı.",
                404,
                "addressNotFound"
            );
        }

        var cart =
            await _context.Carts
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(
                    x => x.UserId == userId
                );

        if (
            cart == null ||
            cart.Items.Count == 0
        )
        {
            throw new AppException(
                "Sepetiniz boş.",
                400,
                "cartIsEmpty"
            );
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,

                Status = OrderStatuses.Pending,

                ShippingFullName =
                    address.FullName,

                ShippingPhone =
                    address.Phone,

                ShippingCity =
                    address.City,

                ShippingDistrict =
                    address.District,

                ShippingAddressLine =
                    address.AddressLine,

                ShippingPostalCode =
                    address.PostalCode
            };

            decimal totalPrice = 0;

            foreach (var cartItem in cart.Items)
            {
                var product =
                    cartItem.Product;

                // Stok kontrolü + stok düşürme
                // TEK ATOMIC DB UPDATE içinde.
                await DecreaseStockAsync(
                    product.Id,
                    cartItem.Quantity,
                    product.Name
                );

                var lineTotal =
                    product.Price *
                    cartItem.Quantity;

                totalPrice +=
                    lineTotal;

                order.Items.Add(
                    new OrderItem
                    {
                        ProductId =
                            product.Id,

                        ProductName =
                            product.Name,

                        UnitPrice =
                            product.Price,

                        Quantity =
                            cartItem.Quantity,

                        LineTotal =
                            lineTotal
                    }
                );
            }

            order.TotalPrice =
                totalPrice;

            _context.Orders.Add(
                order
            );

            _context.CartItems.RemoveRange(
                cart.Items
            );

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await GetByIdAsync(
                userId,
                order.Id
            );
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
    public async Task<List<OrderDto>> GetMyOrdersAsync(
        int userId
    )
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new OrderDto
            {
                Id = x.Id,
                Status = x.Status,
                TotalPrice = x.TotalPrice,
                ShippingFullName =
                    x.ShippingFullName,
                ShippingCity =
                    x.ShippingCity,
                ShippingDistrict =
                    x.ShippingDistrict,
                ShippingAddressLine =
                    x.ShippingAddressLine,
                CreatedAt =
                    x.CreatedAt,

                Items =
                    x.Items.Select(
                        i => new OrderItemDto
                        {
                            ProductId =
                                i.ProductId,

                            ProductName =
                                i.ProductName,

                            UnitPrice =
                                i.UnitPrice,

                            Quantity =
                                i.Quantity,

                            LineTotal =
                                i.LineTotal
                        }
                    ).ToList()
            })
            .ToListAsync();
    }

    public async Task<OrderDto> GetByIdAsync(
        int userId,
        int orderId
    )
    {
        var order =
            await _context.Orders
                .AsNoTracking()
                .Where(
                    x =>
                        x.Id == orderId &&
                        x.UserId == userId
                )
                .Select(x => new OrderDto
                {
                    Id = x.Id,
                    Status = x.Status,
                    TotalPrice = x.TotalPrice,

                    ShippingFullName =
                        x.ShippingFullName,

                    ShippingCity =
                        x.ShippingCity,

                    ShippingDistrict =
                        x.ShippingDistrict,

                    ShippingAddressLine =
                        x.ShippingAddressLine,

                    CreatedAt =
                        x.CreatedAt,

                    Items =
                        x.Items.Select(
                            i => new OrderItemDto
                            {
                                ProductId =
                                    i.ProductId,

                                ProductName =
                                    i.ProductName,

                                UnitPrice =
                                    i.UnitPrice,

                                Quantity =
                                    i.Quantity,

                                LineTotal =
                                    i.LineTotal
                            }
                        ).ToList()
                })
                .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new AppException(
                "Sipariş bulunamadı.",
                404,
                "orderNotFound"
            );
        }

        return order;
    }

    private async Task DecreaseStockAsync(
    int productId,
    int quantity,
    string productName
)
    {
        var affectedRows =
            await _context.Products
                .Where(
                    x =>
                        x.Id == productId &&
                        x.Stock >= quantity
                )
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            x => x.Stock,
                            x => x.Stock - quantity
                        )
                );

        if (affectedRows == 0)
        {
            throw new AppException(
                $"{productName} için yeterli stok bulunmuyor.",
                409,
                "insufficientStock"
            );
        }
    }

    public async Task<OrderDto> CancelAsync(
     int userId,
     int orderId
    )
    {
        return await ChangeStatusAsync(
            orderId,
            OrderStatuses.Cancelled,
            actorUserId: userId,
            customerUserId: userId
        );
    }

    public async Task<OrderDto> UpdateStatusAsync(
        int orderId,
        UpdateOrderStatusDto dto,
        int adminUserId
    )
    {
        var newStatus = NormalizeStatus(dto.Status);

        return await ChangeStatusAsync(
            orderId,
            newStatus,
            actorUserId: adminUserId
        );
    }

    private async Task<OrderDto> ChangeStatusAsync(
            int orderId,
            string newStatus,
            int actorUserId,
            int? customerUserId = null
    )
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var query = _context.Orders
                .AsNoTracking()
                .Where(x => x.Id == orderId);

            // Müşteri yalnızca kendi siparişini değiştirebilir.
            if (customerUserId.HasValue)
            {
                query = query.Where(
                    x => x.UserId == customerUserId.Value
                );
            }

            var order = await query
                .Select(x => new
                {
                    x.Id,
                    x.Status,

                    Items = x.Items.Select(i => new
                    {
                        i.ProductId,
                        i.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new AppException(
                    "Sipariş bulunamadı.",
                    404,
                    "orderNotFound"
                );
            }

            if (order.Status == newStatus)
            {
                throw new AppException(
                    "Sipariş zaten bu durumda.",
                    409,
                    "orderAlreadyInStatus"
                );
            }

            if (!OrderStatusRules.CanTransition(
                order.Status,
                newStatus
            ))
            {
                throw new AppException(
                    $"{order.Status} durumundan {newStatus} durumuna geçilemez.",
                    409,
                    "invalidOrderStatusTransition"
                );
            }

            // Eş zamanlı isteklerde sadece mevcut durumu
            // hâlâ aynı olan işlem başarılı olabilir.
            var affectedRows = await _context.Orders
                .Where(x =>
                    x.Id == orderId &&
                    x.Status == order.Status
                )
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(
                        x => x.Status,
                        newStatus
                    )
                );

            if (affectedRows != 1)
            {
                throw new AppException(
                    "Sipariş durumu başka bir işlem tarafından değiştirildi.",
                    409,
                    "orderStatusConflict"
                );
            }

            // İptal sırasında stokları geri yükle.
            if (newStatus == OrderStatuses.Cancelled)
            {
                foreach (var item in order.Items.OrderBy(
                    x => x.ProductId
                ))
                {
                    var restored = await _context.Products
                        .Where(x => x.Id == item.ProductId)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(
                                x => x.Stock,
                                x => x.Stock + item.Quantity
                            )
                        );

                    if (restored != 1)
                    {
                        throw new AppException(
                            "İptal sırasında ürün stoğu güncellenemedi.",
                            409,
                            "stockRestoreFailed"
                        );
                    }
                }
            }

            var history = new OrderStatusHistory
            {
                OrderId = orderId,

                OldStatus = order.Status,

                NewStatus = newStatus,

                ChangedByUserId = actorUserId,

                ChangedAt = DateTime.UtcNow
            };

            _context.OrderStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        if (customerUserId.HasValue)
        {
            return await GetByIdAsync(
                customerUserId.Value,
                orderId
            );
        }

        return await GetOrderByIdForAdminAsync(
            orderId
        );
    }
    private static string NormalizeStatus(
    string status
)
    {
        if (
            status.Equals(
                OrderStatuses.Pending,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return OrderStatuses.Pending;
        }

        if (
            status.Equals(
                OrderStatuses.Paid,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return OrderStatuses.Paid;
        }

        if (
            status.Equals(
                OrderStatuses.Preparing,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return OrderStatuses.Preparing;
        }

        if (
            status.Equals(
                OrderStatuses.Shipped,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return OrderStatuses.Shipped;
        }

        if (
            status.Equals(
                OrderStatuses.Delivered,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return OrderStatuses.Delivered;
        }

        if (
            status.Equals(
                OrderStatuses.Cancelled,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return OrderStatuses.Cancelled;
        }

        throw new AppException(
            "Geçersiz sipariş durumu.",
            400,
            "invalidOrderStatus"
        );
    }

    private async Task<OrderDto>
    GetOrderByIdForAdminAsync(
        int orderId
    )
    {
        var order =
            await _context.Orders
                .AsNoTracking()
                .Where(
                    x => x.Id == orderId
                )
                .Select(
                    x => new OrderDto
                    {
                        Id = x.Id,

                        Status =
                            x.Status,

                        TotalPrice =
                            x.TotalPrice,

                        ShippingFullName =
                            x.ShippingFullName,

                        ShippingCity =
                            x.ShippingCity,

                        ShippingDistrict =
                            x.ShippingDistrict,

                        ShippingAddressLine =
                            x.ShippingAddressLine,

                        CreatedAt =
                            x.CreatedAt,

                        Items =
                            x.Items
                                .Select(
                                    i =>
                                        new OrderItemDto
                                        {
                                            ProductId =
                                                i.ProductId,

                                            ProductName =
                                                i.ProductName,

                                            UnitPrice =
                                                i.UnitPrice,

                                            Quantity =
                                                i.Quantity,

                                            LineTotal =
                                                i.LineTotal
                                        }
                                )
                                .ToList()
                    }
                )
                .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new AppException(
                "Sipariş bulunamadı.",
                404,
                "orderNotFound"
            );
        }

        return order;
    }

    public async Task<List<AdminOrderDto>>
    GetAllForAdminAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminOrderDto
            {
                Id = x.Id,

                UserId = x.UserId,

                CustomerName = x.User.Name,

                CustomerEmail = x.User.Email,

                Status = x.Status,

                TotalPrice = x.TotalPrice,

                CreatedAt = x.CreatedAt,

                TotalQuantity = x.Items.Sum(
                    i => i.Quantity
                )
            })
            .ToListAsync();
    }

    public async Task<List<OrderStatusHistoryDto>>
    GetStatusHistoryAsync(
        int orderId,
        int requestingUserId,
        bool isAdmin
    )
    {
        var orderExists = await _context.Orders
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == orderId &&
                (isAdmin || x.UserId == requestingUserId)
            );

        if (!orderExists)
        {
            throw new AppException(
                "Sipariş bulunamadı.",
                404,
                "orderNotFound"
            );
        }

        return await _context.OrderStatusHistories
            .AsNoTracking()
            .Where(x => x.OrderId == orderId)
            .OrderBy(x => x.ChangedAt)
            .ThenBy(x => x.Id)
            .Select(x => new OrderStatusHistoryDto
            {
                Id = x.Id,

                OldStatus = x.OldStatus,

                NewStatus = x.NewStatus,

                ChangedByUserId = x.ChangedByUserId,

                ChangedByName =
                    x.ChangedByUser != null
                        ? x.ChangedByUser.Name
                        : null,

                ChangedAt = x.ChangedAt
            })
            .ToListAsync();
    }
}