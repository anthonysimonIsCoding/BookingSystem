using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Net.Http;
using System.Text.Json;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly BookingDbContext _context;
    private readonly string _imgbbApiKey;

    public PetsController(BookingDbContext context)
    {
        _context = context;
        _imgbbApiKey = Environment.GetEnvironmentVariable("IMGBB_API_KEY") ?? "YOUR_KEY_HERE";
    }

    // ====================== UPLOAD ẢNH ======================
    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("Không có file");
        if (file.Length > 10 * 1024 * 1024) return BadRequest("Ảnh phải < 10MB");
        if (!file.ContentType.StartsWith("image/")) return BadRequest("Chỉ upload ảnh");

        try
        {
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "image", file.FileName);

            var response = await client.PostAsync($"https://api.imgbb.com/1/upload?key={_imgbbApiKey}", content);
            var json = await response.Content.ReadAsStringAsync();
            var url = JsonDocument.Parse(json).RootElement.GetProperty("data").GetProperty("url").GetString();
            return Ok(new { url });
        }
        catch { return BadRequest("Upload ảnh thất bại"); }
    }

    public class PetRequest
    {
        public string Name { get; set; } = null!;
        public string Species { get; set; } = null!;
        public string? Breed { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Color { get; set; }
        public double? Weight { get; set; }
        public string? Notes { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPets()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = Guid.Parse(userIdStr);

        var pets = await _context.Pets
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                Species = p.Species.Name,
                Breed = p.Breed != null ? p.Breed.Name : null,
                p.Gender,
                DateOfBirth = p.DateOfBirth.HasValue ? p.DateOfBirth.Value.ToString("yyyy-MM-dd") : null,
                p.Color,
                p.Weight,
                p.Notes,
                p.ProfileImageUrl
            })
            .ToListAsync();

        return Ok(pets);
    }

    [HttpGet("species")]
    public async Task<IActionResult> GetSpecies() => Ok(await _context.Species.Select(s => new { s.Id, s.Name }).ToListAsync());

    [HttpGet("breeds/{speciesId}")]
    public async Task<IActionResult> GetBreeds(Guid speciesId) => Ok(await _context.Breeds
        .Where(b => b.SpeciesId == speciesId)
        .Select(b => new { b.Id, b.Name })
        .ToListAsync());

    // ====================== CREATE ======================
    [HttpPost]
    public async Task<IActionResult> CreatePet([FromBody] PetRequest req)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var species = await _context.Species.FirstOrDefaultAsync(s => s.Name.ToLower() == req.Species.ToLower());
            if (species == null) return BadRequest($"Không tìm thấy loài: {req.Species}");

            Guid? breedId = null;
            if (!string.IsNullOrEmpty(req.Breed))
            {
                var breed = await _context.Breeds.FirstOrDefaultAsync(b => b.Name.ToLower() == req.Breed.ToLower() && b.SpeciesId == species.Id);
                if (breed == null) return BadRequest($"Không tìm thấy giống '{req.Breed}' thuộc loài {req.Species}");
                breedId = breed.Id;
            }

            DateOnly? dob = null;
            if (!string.IsNullOrEmpty(req.DateOfBirth) && DateOnly.TryParse(req.DateOfBirth, out var d)) dob = d;

            var pet = new Pet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = req.Name,
                SpeciesId = species.Id,
                BreedId = breedId,
                Gender = req.Gender,
                DateOfBirth = dob,
                Color = req.Color,
                Weight = req.Weight,
                Notes = req.Notes,
                ProfileImageUrl = req.ProfileImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return Ok(new { pet.Id, pet.Name, Species = req.Species, Breed = req.Breed, pet.ProfileImageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Lỗi server: " + ex.Message);
        }
    }

    // ====================== UPDATE - ĐÃ SỬA (GIỮ ẢNH CŨ KHI KHÔNG CHỌN ẢNH MỚI) ======================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(Guid id, [FromBody] PetRequest req)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (pet == null) return NotFound("Pet not found");

            var species = await _context.Species.FirstOrDefaultAsync(s => s.Name.ToLower() == req.Species.ToLower());
            if (species == null) return BadRequest($"Không tìm thấy loài: {req.Species}");

            Guid? breedId = null;
            if (!string.IsNullOrEmpty(req.Breed))
            {
                var breed = await _context.Breeds.FirstOrDefaultAsync(b => b.Name.ToLower() == req.Breed.ToLower() && b.SpeciesId == species.Id);
                if (breed == null) return BadRequest($"Không tìm thấy giống '{req.Breed}' thuộc loài {req.Species}");
                breedId = breed.Id;
            }

            DateOnly? dob = null;
            if (!string.IsNullOrEmpty(req.DateOfBirth) && DateOnly.TryParse(req.DateOfBirth, out var d))
                dob = d;

            // ==================== PHẦN QUAN TRỌNG ====================
            pet.Name = req.Name;
            pet.SpeciesId = species.Id;
            pet.BreedId = breedId;
            pet.Gender = req.Gender;
            pet.DateOfBirth = dob;
            pet.Color = req.Color;
            pet.Weight = req.Weight;
            pet.Notes = req.Notes;

            // CHỈ cập nhật ảnh khi có URL mới (không rỗng)
            if (!string.IsNullOrEmpty(req.ProfileImageUrl))
                pet.ProfileImageUrl = req.ProfileImageUrl;

            pet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { pet.Id, pet.Name, Species = req.Species, Breed = req.Breed, pet.ProfileImageUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Lỗi server: " + ex.Message);
        }
    }

    // ====================== LẤY TRẠNG THÁI BOOKING MỚI NHẤT CỦA PET ======================
    [HttpGet("{petId}/latest-booking")]
    public async Task<IActionResult> GetLatestBooking(Guid petId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Store)
            .Where(b => b.PetId == petId)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (booking == null)
            return Ok(null);

        return Ok(new
        {
            storeName = booking.Store.Name,
            status = booking.Status,           // 0 = Active, 1 = Cancelled, 2 = Completed
            bookingDate = booking.BookingDate.ToString("yyyy-MM-dd")
        });
    }
}