using Microsoft.EntityFrameworkCore;
using ShopApi.Data;
using ShopApi.Dtos.Products;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;
using ShopApi.Models.Responses;

namespace ShopApi.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;

    public ProductService(
        AppDbContext context,
        IImageService imageService
    )
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<PagedResult<ProductDto>> GetAllAsync(
    ProductQueryDto request
)
    {
        var page =
            request.Page < 1
                ? 1
                : request.Page;

        var pageSize =
            request.PageSize switch
            {
                < 1 => 20,
                > 100 => 100,
                _ => request.PageSize
            };

        if (
            request.MinPrice.HasValue &&
            request.MinPrice.Value < 0
        )
        {
            throw new AppException(
                "Minimum fiyat negatif olamaz.",
                400,
                "minPriceInvalid"
            );
        }

        if (
            request.MaxPrice.HasValue &&
            request.MaxPrice.Value < 0
        )
        {
            throw new AppException(
                "Maksimum fiyat negatif olamaz.",
                400,
                "maxPriceInvalid"
            );
        }

        if (
            request.MinPrice.HasValue &&
            request.MaxPrice.HasValue &&
            request.MinPrice.Value >
            request.MaxPrice.Value
        )
        {
            throw new AppException(
                "Minimum fiyat maksimum fiyattan büyük olamaz.",
                400,
                "priceRangeInvalid"
            );
        }

        var query =
            _context.Products
                .AsNoTracking()
                .AsQueryable();

        if (
            !string.IsNullOrWhiteSpace(
                request.Search
            )
        )
        {
            var search =
                request.Search.Trim();

            query = query.Where(
                x =>
                    EF.Functions.ILike(
                        x.Name,
                        $"%{search}%"
                    )
                    ||
                    EF.Functions.ILike(
                        x.Description,
                        $"%{search}%"
                    )
            );
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(
                x =>
                    x.CategoryId ==
                    request.CategoryId.Value
            );
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(
                x =>
                    x.Price >=
                    request.MinPrice.Value
            );
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(
                x =>
                    x.Price <=
                    request.MaxPrice.Value
            );
        }

        var totalCount =
            await query.CountAsync();

        var products = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Stock = x.Stock,
                CreatedAt = x.CreatedAt,

                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,

                Images = x.Images
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.Id)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    })
                    .ToList(),

                ReviewCount = x.Reviews.Count(),

                AverageRating = x.Reviews
                    .Select(r => (double?)r.Rating)
                    .Average() ?? 0
            })
            .ToListAsync();

        foreach (var product in products)
        {
            product.AverageRating =
                Math.Round(product.AverageRating, 2);
        }
        var totalPages =
            (int)Math.Ceiling(
                totalCount /
                (double)pageSize
            );

        return new PagedResult<ProductDto>
        {
            Items = products,

            Pagination =
                new PaginationMeta
                {
                    Page = page,

                    PageSize = pageSize,

                    TotalCount =
                        totalCount,

                    TotalPages =
                        totalPages
                }
        };
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Stock = x.Stock,
                CreatedAt = x.CreatedAt,

                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,

                Images = x.Images
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.Id)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    })
                    .ToList(),

                ReviewCount = x.Reviews.Count(),

                AverageRating = x.Reviews
                    .Select(r => (double?)r.Rating)
                    .Average() ?? 0
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        product.AverageRating =
            Math.Round(product.AverageRating, 2);

        return product;
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductDto dto
    )
    {
        var categoryExists =
            await _context.Categories
                .AnyAsync(
                    x => x.Id == dto.CategoryId
                );

        if (!categoryExists)
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404,
                "categoryNotFound"
            );
        }

        var name =
            dto.Name.Trim();

        var productExists =
            await _context.Products
                .AnyAsync(
                    x =>
                        x.Name.ToLower() ==
                        name.ToLower()
                );

        if (productExists)
        {
            throw new AppException(
                "Bu isimde bir ürün zaten mevcut.",
                409,
                "productAlreadyExists"
            );
        }

        var product = new Product
        {
            Name = name,

            Description =
                dto.Description.Trim(),

            Price = dto.Price,

            Stock = dto.Stock,

            CategoryId =
                dto.CategoryId
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(
            product.Id
        );
    }

    public async Task<ProductDto> UpdateAsync(
        int id,
        UpdateProductDto dto
    )
    {
        var product =
            await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Id == id
                );

        if (product == null)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        var categoryExists =
            await _context.Categories
                .AnyAsync(
                    x => x.Id == dto.CategoryId
                );

        if (!categoryExists)
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404,
                "categoryNotFound"
            );
        }

        var name =
            dto.Name.Trim();

        var nameExists =
            await _context.Products
                .AnyAsync(
                    x =>
                        x.Id != id &&
                        x.Name.ToLower() ==
                        name.ToLower()
                );

        if (nameExists)
        {
            throw new AppException(
                "Bu isimde başka bir ürün zaten mevcut.",
                409,
                "productAlreadyExists"
            );
        }

        product.Name = name;

        product.Description =
            dto.Description.Trim();

        product.Price =
            dto.Price;

        product.Stock =
            dto.Stock;

        product.CategoryId =
            dto.CategoryId;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(
            product.Id
        );
    }

    public async Task DeleteAsync(
        int id
    )
    {
        var product =
            await _context.Products
                .Include(x => x.Images)
                .FirstOrDefaultAsync(
                    x => x.Id == id
                );

        if (product == null)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        foreach (var image in product.Images)
        {
            await _imageService.DeleteAsync(
                image.PublicId
            );
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }

    private static ProductDto MapToDto(
        Product product
    )
    {
        return new ProductDto
        {
            Id = product.Id,

            Name = product.Name,

            Description =
                product.Description,

            Price = product.Price,

            Stock = product.Stock,

            CreatedAt =
                product.CreatedAt,

            CategoryId =
                product.CategoryId,

            CategoryName =
                product.Category.Name,

            Images = product.Images
                .OrderByDescending(
                    x => x.IsMain
                )
                .ThenBy(x => x.Id)
                .Select(x =>
                    new ProductImageDto
                    {
                        Id = x.Id,
                        ImageUrl =
                            x.ImageUrl,
                        IsMain =
                            x.IsMain
                    }
                )
                .ToList()
        };
    }

    public async Task<ProductImageDto> UploadImageAsync(
    int productId,
    UploadProductImageDto dto
)
    {
        var product =
            await _context.Products
                .Include(x => x.Images)
                .FirstOrDefaultAsync(
                    x => x.Id == productId
                );

        if (product == null)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        var uploadResult =
            await _imageService.UploadAsync(
                dto.File
            );

        try
        {
            var shouldBeMain =
                dto.IsMain ||
                product.Images.Count == 0;

            if (shouldBeMain)
            {
                foreach (var existingImage in product.Images)
                {
                    existingImage.IsMain = false;
                }
            }

            var productImage =
                new ProductImage
                {
                    ImageUrl =
                        uploadResult.ImageUrl,

                    PublicId =
                        uploadResult.PublicId,

                    IsMain =
                        shouldBeMain,

                    ProductId =
                        product.Id
                };

            _context.ProductImages.Add(
                productImage
            );

            await _context.SaveChangesAsync();

            return new ProductImageDto
            {
                Id = productImage.Id,

                ImageUrl =
                    productImage.ImageUrl,

                IsMain =
                    productImage.IsMain
            };
        }
        catch
        {
            await _imageService.DeleteAsync(
                uploadResult.PublicId
            );

            throw;
        }
    }

    public async Task DeleteImageAsync(
    int productId,
    int imageId
)
    {
        var product =
            await _context.Products
                .Include(x => x.Images)
                .FirstOrDefaultAsync(
                    x => x.Id == productId
                );

        if (product == null)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        var image =
            product.Images
                .FirstOrDefault(
                    x => x.Id == imageId
                );

        if (image == null)
        {
            throw new AppException(
                "Ürün fotoğrafı bulunamadı.",
                404,
                "productImageNotFound"
            );
        }

        var wasMainImage =
            image.IsMain;

        await _imageService.DeleteAsync(
            image.PublicId
        );

        _context.ProductImages.Remove(
            image
        );

        if (wasMainImage)
        {
            var newMainImage =
                product.Images
                    .Where(x => x.Id != imageId)
                    .OrderBy(x => x.Id)
                    .FirstOrDefault();

            if (newMainImage != null)
            {
                newMainImage.IsMain = true;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<ProductImageDto> SetMainImageAsync(
    int productId,
    int imageId
)
    {
        var product =
            await _context.Products
                .Include(x => x.Images)
                .FirstOrDefaultAsync(
                    x => x.Id == productId
                );

        if (product == null)
        {
            throw new AppException(
                "Ürün bulunamadı.",
                404,
                "productNotFound"
            );
        }

        var image =
            product.Images
                .FirstOrDefault(
                    x => x.Id == imageId
                );

        if (image == null)
        {
            throw new AppException(
                "Ürün fotoğrafı bulunamadı.",
                404,
                "productImageNotFound"
            );
        }

        foreach (var productImage in product.Images)
        {
            productImage.IsMain =
                productImage.Id == imageId;
        }

        await _context.SaveChangesAsync();

        return new ProductImageDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            IsMain = image.IsMain
        };
    }
}