using Microsoft.EntityFrameworkCore;
using ShopApi.Data;
using ShopApi.Dtos.Categories;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;

namespace ShopApi.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                ProductCount = x.Products.Count
            })
            .ToListAsync();
    }

    public async Task<CategoryDto> GetByIdAsync(
        int id
    )
    {
        var category =
            await _context.Categories
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = x.Products.Count
                })
                .FirstOrDefaultAsync();

        if (category == null)
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404,
                "categoryNotFound"
            );
        }

        return category;
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto dto
    )
    {
        var name =
            dto.Name.Trim();

        var exists =
            await _context.Categories
                .AnyAsync(
                    x => x.Name.ToLower() ==
                         name.ToLower()
                );

        if (exists)
        {
            throw new AppException(
                "Bu kategori zaten mevcut.",
                409,
                "categoryAlreadyExists"
            );
        }

        var category = new Category
        {
            Name = name
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            ProductCount = 0
        };
    }

    public async Task<CategoryDto> UpdateAsync(
        int id,
        UpdateCategoryDto dto
    )
    {
        var category =
            await _context.Categories
                .FirstOrDefaultAsync(
                    x => x.Id == id
                );

        if (category == null)
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
            await _context.Categories
                .AnyAsync(
                    x =>
                        x.Id != id &&
                        x.Name.ToLower() ==
                        name.ToLower()
                );

        if (nameExists)
        {
            throw new AppException(
                "Bu kategori adı zaten kullanılıyor.",
                409,
                "categoryAlreadyExists"
            );
        }

        category.Name = name;

        await _context.SaveChangesAsync();

        var productCount =
            await _context.Products.CountAsync(
                x => x.CategoryId == id
            );

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            ProductCount = productCount
        };
    }

    public async Task DeleteAsync(
        int id
    )
    {
        var category =
            await _context.Categories
                .Include(x => x.Products)
                .FirstOrDefaultAsync(
                    x => x.Id == id
                );

        if (category == null)
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404,
                "categoryNotFound"
            );
        }

        if (category.Products.Count > 0)
        {
            throw new AppException(
                "İçerisinde ürün bulunan kategori silinemez.",
                409,
                "categoryHasProducts"
            );
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
    }
}