using Microsoft.EntityFrameworkCore;
using ShopApi.Data;
using ShopApi.Dtos.Orders;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;

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

                Status = "Pending",

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
}