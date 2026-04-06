using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingSystem.Data;
using BookingSystem.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Admin;

//[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/masterdata")]
public class AdminMasterDataController : ControllerBase
{
    private readonly BookingDbContext _context;

    public AdminMasterDataController(BookingDbContext context)
    {
        _context = context;
    }

    #region ==================== STORE CATEGORY ====================

    [HttpGet("store-categories")]
    public async Task<IActionResult> GetStoreCategories()
    {
        var list = await _context.StoreCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("store-categories")]
    public async Task<IActionResult> CreateStoreCategory([FromBody] StoreCategory category)
    {
        category.CreatedAt = DateTime.UtcNow;
        _context.StoreCategories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(category);
    }

    [HttpPut("store-categories/{id}")]
    public async Task<IActionResult> UpdateStoreCategory(Guid id, [FromBody] StoreCategory category)
    {
        var existing = await _context.StoreCategories.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.IsActive = category.IsActive;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("store-categories/{id}")]
    public async Task<IActionResult> DeleteStoreCategory(Guid id)
    {
        var category = await _context.StoreCategories.FindAsync(id);
        if (category == null) return NotFound();

        _context.StoreCategories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã xóa danh mục" });
    }

    #endregion

    #region ==================== SPECIES ====================

    [HttpGet("species")]
    public async Task<IActionResult> GetSpecies()
    {
        var list = await _context.Species
            .OrderBy(s => s.Name)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("species")]
    public async Task<IActionResult> CreateSpecies([FromBody] Species species)
    {
        species.CreatedAt = DateTime.UtcNow;
        _context.Species.Add(species);
        await _context.SaveChangesAsync();
        return Ok(species);
    }

    [HttpPut("species/{id}")]
    public async Task<IActionResult> UpdateSpecies(Guid id, [FromBody] Species species)
    {
        var existing = await _context.Species.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = species.Name;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("species/{id}")]
    public async Task<IActionResult> DeleteSpecies(Guid id)
    {
        var species = await _context.Species.FindAsync(id);
        if (species == null) return NotFound();

        _context.Species.Remove(species);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã xóa loài" });
    }

    #endregion

    #region ==================== BREED ====================

    [HttpGet("breeds")]
    public async Task<IActionResult> GetBreeds()
    {
        var breeds = await _context.Breeds
            .Include(b => b.Species)
            .AsNoTracking()
            .OrderBy(b => b.Species.Name)
            .ThenBy(b => b.Name)
            .ToListAsync();

        // Project để tránh cycle
        var result = breeds.Select(b => new
        {
            b.Id,
            b.Name,
            b.CreatedAt,
            Species = new
            {
                b.Species.Id,
                b.Species.Name
            }
        });

        return Ok(result);
    }

    [HttpPost("breeds")]
    public async Task<IActionResult> CreateBreed([FromBody] BreedCreateDto dto)
    {
        var breed = new Breed
        {
            Name = dto.Name,
            SpeciesId = dto.SpeciesId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Breeds.Add(breed);
        await _context.SaveChangesAsync();

        // Trả về dữ liệu có Species để frontend dễ hiển thị
        var result = await _context.Breeds
            .Include(b => b.Species)
            .FirstAsync(b => b.Id == breed.Id);

        return Ok(new
        {
            result.Id,
            result.Name,
            result.CreatedAt,
            Species = new { result.Species.Id, result.Species.Name }
        });
    }

    [HttpPut("breeds/{id}")]
    public async Task<IActionResult> UpdateBreed(Guid id, [FromBody] BreedCreateDto dto)
    {
        var breed = await _context.Breeds.FindAsync(id);
        if (breed == null) return NotFound();

        breed.Name = dto.Name;
        breed.SpeciesId = dto.SpeciesId;

        await _context.SaveChangesAsync();

        var result = await _context.Breeds
            .Include(b => b.Species)
            .FirstAsync(b => b.Id == breed.Id);

        return Ok(new
        {
            result.Id,
            result.Name,
            result.CreatedAt,
            Species = new { result.Species.Id, result.Species.Name }
        });
    }

    

    [HttpDelete("breeds/{id}")]
    public async Task<IActionResult> DeleteBreed(Guid id)
    {
        var breed = await _context.Breeds.FindAsync(id);
        if (breed == null) return NotFound();

        _context.Breeds.Remove(breed);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã xóa giống" });
    }

    #endregion
}

// DTO mới (thêm vào cuối file)
public class BreedCreateDto
{
    public string Name { get; set; } = null!;
    public Guid SpeciesId { get; set; }
}