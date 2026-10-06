using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Addresses;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(
        IAddressService addressService
    )
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result =
            await _addressService.GetAllAsync(
                GetUserId()
            );

        return Ok(
            ApiResponse<List<AddressDto>>.Success(
                result,
                200,
                "Adresler getirildi."
            )
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateAddressDto dto
    )
    {
        var result =
            await _addressService.CreateAsync(
                GetUserId(),
                dto
            );

        return StatusCode(
            201,
            ApiResponse<AddressDto>.Success(
                result,
                201,
                "Adres oluşturuldu."
            )
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateAddressDto dto
    )
    {
        var result =
            await _addressService.UpdateAsync(
                GetUserId(),
                id,
                dto
            );

        return Ok(
            ApiResponse<AddressDto>.Success(
                result,
                200,
                "Adres güncellendi."
            )
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id
    )
    {
        await _addressService.DeleteAsync(
            GetUserId(),
            id
        );

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Adres silindi."
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