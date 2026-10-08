using Microsoft.EntityFrameworkCore;
using ShopApi.Constants;
using ShopApi.Data;
using ShopApi.Dtos.Reviews;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;

namespace ShopApi.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewDto>> GetByProductAsync(
        int productId
    )
    {
        var productExists = await _context.Products
            .AnyAsync(x => x.Id == productId);

        if (!productExists)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        return await _context.Reviews
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ReviewDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                UserId = x.UserId,
                UserName = x.User.Name,
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ReviewDto> CreateAsync(
        int userId,
        int productId,
        CreateReviewDto dto
    )
    {
        var productExists = await _context.Products
            .AnyAsync(x => x.Id == productId);

        if (!productExists)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        var hasPurchased = await _context.Orders
            .AnyAsync(order =>
                order.UserId == userId &&
                order.Status != OrderStatuses.Cancelled &&
                order.Items.Any(item =>
                    item.ProductId == productId
                )
            );

        if (!hasPurchased)
        {
            throw new AppException(
                "Bu ürünü değerlendirmek için satın almış olmalısınız.",
                403,
                "productNotPurchased"
            );
        }

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(x =>
                x.UserId == userId &&
                x.ProductId == productId
            );

        if (alreadyReviewed)
        {
            throw new AppException(
                "Bu ürünü zaten değerlendirdiniz.",
                409,
                "reviewAlreadyExists"
            );
        }

        var review = new Review
        {
            UserId = userId,
            ProductId = productId,
            Rating = dto.Rating,
            Comment = dto.Comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);

        await _context.SaveChangesAsync();

        return await GetReviewByIdAsync(review.Id);
    }

    public async Task<ReviewDto> UpdateAsync(
        int userId,
        int reviewId,
        UpdateReviewDto dto
    )
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(x =>
                x.Id == reviewId &&
                x.UserId == userId
            );

        if (review == null)
        {
            throw new AppException(
                "Yorum bulunamadı.",
                404,
                "reviewNotFound"
            );
        }

        review.Rating = dto.Rating;
        review.Comment = dto.Comment?.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetReviewByIdAsync(review.Id);
    }

    public async Task DeleteAsync(
        int userId,
        int reviewId
    )
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(x =>
                x.Id == reviewId &&
                x.UserId == userId
            );

        if (review == null)
        {
            throw new AppException(
                "Yorum bulunamadı.",
                404,
                "reviewNotFound"
            );
        }

        _context.Reviews.Remove(review);

        await _context.SaveChangesAsync();
    }

    private async Task<ReviewDto> GetReviewByIdAsync(int id)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ReviewDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                UserId = x.UserId,
                UserName = x.User.Name,
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstAsync();
    }

    public async Task<ReviewSummaryDto> GetSummaryAsync(
        int productId
    )
    {
        var productExists = await _context.Products
            .AnyAsync(x => x.Id == productId);

        if (!productExists)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        var summary = await _context.Reviews
            .Where(x => x.ProductId == productId)
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ReviewCount = g.Count(),
                AverageRating = g.Average(x => (double)x.Rating)
            })
            .FirstOrDefaultAsync();

        return new ReviewSummaryDto
        {
            ProductId = productId,

            AverageRating = summary == null
                ? 0
                : Math.Round(summary.AverageRating, 2),

            ReviewCount = summary?.ReviewCount ?? 0
        };
    }
}