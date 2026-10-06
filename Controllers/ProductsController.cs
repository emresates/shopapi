using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Products;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(
        IProductService productService
    )
    {
        _productService = productService;
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] ProductQueryDto query
    )
    {
        var result =
            await _productService
                .GetAllAsync(query);

        return Ok(
            ApiResponse<List<ProductDto>>
                .Success(
                    result.Items,
                    200,
                    "Ürünler getirildi.",
                    result.Pagination
                )
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id
    )
    {
        var result =
            await _productService.GetByIdAsync(id);

        return Ok(
            ApiResponse<ProductDto>.Success(
                result,
                200,
                "Ürün getirildi."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
            CreateProductDto dto
        )
    {
        var result =
            await _productService.CreateAsync(dto);

        return StatusCode(
            201,
            ApiResponse<ProductDto>.Success(
                result,
                201,
                "Ürün oluşturuldu."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
            int id,
            UpdateProductDto dto
        )
    {
        var result =
            await _productService.UpdateAsync(
                id,
                dto
            );

        return Ok(
            ApiResponse<ProductDto>.Success(
                result,
                200,
                "Ürün güncellendi."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
            int id
        )
    {
        await _productService.DeleteAsync(id);

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Ürün silindi."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{productId:int}/images")]
    public async Task<IActionResult> UploadImage(
        int productId,
        [FromForm] UploadProductImageDto dto
        )
    {
        var result =
            await _productService.UploadImageAsync(
                productId,
                dto
            );

        return StatusCode(
            201,
            ApiResponse<ProductImageDto>.Success(
                result,
                201,
                "Ürün fotoğrafı yüklendi."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(
        int productId,
        int imageId
    )
    {
        await _productService.DeleteImageAsync(
            productId,
            imageId
        );

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Ürün fotoğrafı silindi."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{productId:int}/images/{imageId:int}/main")]
    public async Task<IActionResult> SetMainImage(
        int productId,
        int imageId
    )
    {
        var result =
            await _productService.SetMainImageAsync(
                productId,
                imageId
            );

        return Ok(
            ApiResponse<ProductImageDto>.Success(
                result,
                200,
                "Ana ürün fotoğrafı güncellendi."
            )
        );
    }
}