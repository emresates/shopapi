using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Orders;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(
        IOrderService orderService
    )
    {
        _orderService =
            orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderDto dto
    )
    {
        var result =
            await _orderService.CreateAsync(
                GetUserId(),
                dto
            );

        return StatusCode(
            201,
            ApiResponse<OrderDto>.Success(
                result,
                201,
                "Sipariş oluşturuldu."
            )
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var result =
            await _orderService
                .GetMyOrdersAsync(
                    GetUserId()
                );

        return Ok(
            ApiResponse<List<OrderDto>>.Success(
                result,
                200,
                "Siparişler getirildi."
            )
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id
    )
    {
        var result =
            await _orderService.GetByIdAsync(
                GetUserId(),
                id
            );

        return Ok(
            ApiResponse<OrderDto>.Success(
                result,
                200,
                "Sipariş getirildi."
            )
        );
    }

    private int GetUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (!int.TryParse(value, out var id))
        {
            throw new AppException(
                "Geçersiz kullanıcı.",
                401,
                "invalidToken"
            );
        }

        return id;
    }
}