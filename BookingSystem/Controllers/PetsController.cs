using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookingSystem.Services;
using BookingSystem.DTOs;   // Giả sử PetRequest nằm trong DTOs

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly PetService _petService;

    public PetsController(PetService petService)
    {
        _petService = petService;
    }

    // ====================== UPLOAD ẢNH ======================
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            var url = await _petService.UploadImageAsync(file);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPets()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pets = await _petService.GetMyPetsAsync(userId);
        return Ok(pets);
    }

    [HttpGet("species")]
    public async Task<IActionResult> GetSpecies()
    {
        var species = await _petService.GetSpeciesAsync();
        return Ok(species);
    }

    [HttpGet("breeds/{speciesId}")]
    public async Task<IActionResult> GetBreeds(Guid speciesId)
    {
        var breeds = await _petService.GetBreedsBySpeciesAsync(speciesId);
        return Ok(breeds);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePet([FromBody] PetRequest req)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _petService.CreatePetAsync(req, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(Guid id, [FromBody] PetRequest req)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _petService.UpdatePetAsync(id, req, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    [HttpGet("{petId}/latest-booking")]
    public async Task<IActionResult> GetLatestBooking(Guid petId)
    {
        try
        {
            var result = await _petService.GetLatestBookingAsync(petId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }
}