using Microsoft.EntityFrameworkCore;
using ShopApi.Data;
using ShopApi.Dtos.Addresses;
using ShopApi.Exceptions;
using ShopApi.Interfaces;
using ShopApi.Models;

namespace ShopApi.Services;

public class AddressService : IAddressService
{
    private readonly AppDbContext _context;

    public AddressService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<List<AddressDto>> GetAllAsync(
        int userId
    )
    {
        return await _context.Addresses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<AddressDto> CreateAsync(
        int userId,
        CreateAddressDto dto
    )
    {
        var hasAddress =
            await _context.Addresses
                .AnyAsync(x => x.UserId == userId);

        var shouldBeDefault =
            dto.IsDefault || !hasAddress;

        if (shouldBeDefault)
        {
            await ClearDefaultAddressesAsync(
                userId
            );
        }

        var address = new Address
        {
            Title = dto.Title.Trim(),
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone.Trim(),
            City = dto.City.Trim(),
            District = dto.District.Trim(),
            AddressLine = dto.AddressLine.Trim(),
            PostalCode = dto.PostalCode?.Trim(),
            IsDefault = shouldBeDefault,
            UserId = userId
        };

        _context.Addresses.Add(address);

        await _context.SaveChangesAsync();

        return MapToDto(address);
    }

    public async Task<AddressDto> UpdateAsync(
        int userId,
        int addressId,
        UpdateAddressDto dto
    )
    {
        var address =
            await _context.Addresses
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == addressId &&
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

        if (dto.IsDefault)
        {
            await ClearDefaultAddressesAsync(
                userId,
                address.Id
            );
        }

        address.Title = dto.Title.Trim();
        address.FullName = dto.FullName.Trim();
        address.Phone = dto.Phone.Trim();
        address.City = dto.City.Trim();
        address.District = dto.District.Trim();
        address.AddressLine = dto.AddressLine.Trim();
        address.PostalCode = dto.PostalCode?.Trim();
        address.IsDefault = dto.IsDefault;

        await _context.SaveChangesAsync();

        return MapToDto(address);
    }

    public async Task DeleteAsync(
        int userId,
        int addressId
    )
    {
        var address =
            await _context.Addresses
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == addressId &&
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

        _context.Addresses.Remove(address);

        await _context.SaveChangesAsync();
    }

    private async Task ClearDefaultAddressesAsync(
        int userId,
        int? excludeAddressId = null
    )
    {
        var query =
            _context.Addresses.Where(
                x =>
                    x.UserId == userId &&
                    x.IsDefault
            );

        if (excludeAddressId.HasValue)
        {
            query = query.Where(
                x =>
                    x.Id !=
                    excludeAddressId.Value
            );
        }

        var addresses =
            await query.ToListAsync();

        foreach (var address in addresses)
        {
            address.IsDefault = false;
        }
    }

    private static AddressDto MapToDto(
        Address address
    )
    {
        return new AddressDto
        {
            Id = address.Id,
            Title = address.Title,
            FullName = address.FullName,
            Phone = address.Phone,
            City = address.City,
            District = address.District,
            AddressLine = address.AddressLine,
            PostalCode = address.PostalCode,
            IsDefault = address.IsDefault
        };
    }
}