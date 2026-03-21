using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly BookingDbContext _context;

    public PetsController(BookingDbContext context)
    {
        _context = context;
    }

    // GET api/pets
    [HttpGet]
    public async Task<IActionResult> GetMyPets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        var pets = await _context.Pets
            .Where(p => p.UserId == Guid.Parse(userId))
            .ToListAsync();

        return Ok(pets);
    }

    // PUT api/pets/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(Guid id, [FromBody] Pet updatedPet)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == Guid.Parse(userId));

        if (pet == null)
            return NotFound("Pet not found");

        pet.Name = updatedPet.Name;
        pet.Species = updatedPet.Species;
        pet.Breed = updatedPet.Breed;
        pet.Gender = updatedPet.Gender;
        pet.DateOfBirth = updatedPet.DateOfBirth;
        pet.Color = updatedPet.Color;
        pet.Weight = updatedPet.Weight;
        pet.Notes = updatedPet.Notes;
        pet.ProfileImageUrl = updatedPet.ProfileImageUrl;
        pet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(pet);
    }
}