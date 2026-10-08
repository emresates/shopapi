using ShopApi.Dtos.Reviews;

namespace ShopApi.Interfaces;

public interface IReviewService
{
    Task<List<ReviewDto>> GetByProductAsync(
        int productId
    );

    Task<ReviewDto> CreateAsync(
        int userId,
        int productId,
        CreateReviewDto dto
    );

    Task<ReviewDto> UpdateAsync(
        int userId,
        int reviewId,
        UpdateReviewDto dto
    );

    Task DeleteAsync(
        int userId,
        int reviewId
    );
}