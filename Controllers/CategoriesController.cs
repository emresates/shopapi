using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Categories;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService
    )
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result =
            await _categoryService.GetAllAsync();

        return Ok(
            ApiResponse<List<CategoryDto>>.Success(
                result,
                200,
                "Kategoriler getirildi."
            )
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id
    )
    {
        var result =
            await _categoryService.GetByIdAsync(id);

        return Ok(
            ApiResponse<CategoryDto>.Success(
                result,
                200,
                "Kategori getirildi."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
    CreateCategoryDto dto
)
    {
        var result =
            await _categoryService.CreateAsync(dto);

        return StatusCode(
            201,
            ApiResponse<CategoryDto>.Success(
                result,
                201,
                "Kategori oluşturuldu."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
            int id,
            UpdateCategoryDto dto
        )
    {
        var result =
            await _categoryService.UpdateAsync(
                id,
                dto
            );

        return Ok(
            ApiResponse<CategoryDto>.Success(
                result,
                200,
                "Kategori güncellendi."
            )
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
            int id
        )
    {
        await _categoryService.DeleteAsync(id);

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Kategori silindi."
            )
        );
    }
}