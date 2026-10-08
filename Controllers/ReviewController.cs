using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApi.Dtos.Reviews;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models.Responses;

namespace ShopApi.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetByProduct(
        int productId
    )
    {
        var result = await _reviewService
            .GetByProductAsync(productId);

        return Ok(
            ApiResponse<List<ReviewDto>>.Success(
                result,
                200,
                "Ürün değerlendirmeleri getirildi."
            )
        );
    }

    [Authorize]
    [HttpPost("product/{productId:int}")]
    public async Task<IActionResult> Create(
        int productId,
        CreateReviewDto dto
    )
    {
        var result = await _reviewService.CreateAsync(
            GetUserId(),
            productId,
            dto
        );

        return StatusCode(
            201,
            ApiResponse<ReviewDto>.Success(
                result,
                201,
                "Değerlendirme oluşturuldu."
            )
        );
    }

    [Authorize]
    [HttpPut("{reviewId:int}")]
    public async Task<IActionResult> Update(
        int reviewId,
        UpdateReviewDto dto
    )
    {
        var result = await _reviewService.UpdateAsync(
            GetUserId(),
            reviewId,
            dto
        );

        return Ok(
            ApiResponse<ReviewDto>.Success(
                result,
                200,
                "Değerlendirme güncellendi."
            )
        );
    }

    [Authorize]
    [HttpDelete("{reviewId:int}")]
    public async Task<IActionResult> Delete(int reviewId)
    {
        await _reviewService.DeleteAsync(
            GetUserId(),
            reviewId
        );

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Değerlendirme silindi."
            )
        );
    }

    private int GetUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (!int.TryParse(value, out var userId))
        {
            throw new AppException(
                "Geçersiz kullanıcı.",
                401,
                "invalidToken"
            );
        }

        return userId;
    }

    [HttpGet("product/{productId:int}/summary")]
    public async Task<IActionResult> GetSummary(
    int productId
    )
    {
        var result = await _reviewService
            .GetSummaryAsync(productId);

        return Ok(
            ApiResponse<ReviewSummaryDto>.Success(
                result,
                200,
                "Ürün değerlendirme özeti getirildi."
            )
        );
    }
}