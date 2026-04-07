using BookingSystem.Data;
using BookingSystem.DTOs;
using BookingSystem.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class AdminMasterDataService
{
    private readonly BookingDbContext _context;

    public AdminMasterDataService(BookingDbContext context)
    {
        _context = context;
    }

    #region ==================== STORE CATEGORY ====================

    public async Task<List<StoreCategory>> GetStoreCategoriesAsync()
    {
        return await _context.StoreCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<StoreCategory> CreateStoreCategoryAsync(StoreCategory category)
    {
        category.CreatedAt = DateTime.UtcNow;
        _context.StoreCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<StoreCategory> UpdateStoreCategoryAsync(Guid id, StoreCategory category)
    {
        var existing = await _context.StoreCategories.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Không tìm thấy danh mục");

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.IsActive = category.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteStoreCategoryAsync(Guid id)
    {
        var category = await _context.StoreCategories.FindAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Không tìm thấy danh mục");

        _context.StoreCategories.Remove(category);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region ==================== SPECIES ====================

    public async Task<List<Species>> GetSpeciesAsync()
    {
        return await _context.Species
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Species> CreateSpeciesAsync(Species species)
    {
        species.CreatedAt = DateTime.UtcNow;
        _context.Species.Add(species);
        await _context.SaveChangesAsync();
        return species;
    }

    public async Task<Species> UpdateSpeciesAsync(Guid id, Species species)
    {
        var existing = await _context.Species.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Không tìm thấy loài");

        existing.Name = species.Name;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteSpeciesAsync(Guid id)
    {
        var species = await _context.Species.FindAsync(id);
        if (species == null)
            throw new KeyNotFoundException("Không tìm thấy loài");

        _context.Species.Remove(species);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region ==================== BREED ====================

    public async Task<List<object>> GetBreedsAsync()
    {
        var breeds = await _context.Breeds
            .Include(b => b.Species)
            .AsNoTracking()
            .OrderBy(b => b.Species.Name)
            .ThenBy(b => b.Name)
            .ToListAsync();

        return breeds.Select(b => new
        {
            b.Id,
            b.Name,
            b.CreatedAt,
            Species = new
            {
                b.Species.Id,
                b.Species.Name
            }
        }).ToList<object>();
    }

    public async Task<object> CreateBreedAsync(BreedCreateDto dto)
    {
        var breed = new Breed
        {
            Name = dto.Name,
            SpeciesId = dto.SpeciesId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Breeds.Add(breed);
        await _context.SaveChangesAsync();

        // Trả về kèm Species
        var result = await _context.Breeds
            .Include(b => b.Species)
            .FirstAsync(b => b.Id == breed.Id);

        return new
        {
            result.Id,
            result.Name,
            result.CreatedAt,
            Species = new { result.Species.Id, result.Species.Name }
        };
    }

    public async Task<object> UpdateBreedAsync(Guid id, BreedCreateDto dto)
    {
        var breed = await _context.Breeds.FindAsync(id);
        if (breed == null)
            throw new KeyNotFoundException("Không tìm thấy giống");

        breed.Name = dto.Name;
        breed.SpeciesId = dto.SpeciesId;

        await _context.SaveChangesAsync();

        var result = await _context.Breeds
            .Include(b => b.Species)
            .FirstAsync(b => b.Id == breed.Id);

        return new
        {
            result.Id,
            result.Name,
            result.CreatedAt,
            Species = new { result.Species.Id, result.Species.Name }
        };
    }

    public async Task DeleteBreedAsync(Guid id)
    {
        var breed = await _context.Breeds.FindAsync(id);
        if (breed == null)
            throw new KeyNotFoundException("Không tìm thấy giống");

        _context.Breeds.Remove(breed);
        await _context.SaveChangesAsync();
    }

    #endregion
}