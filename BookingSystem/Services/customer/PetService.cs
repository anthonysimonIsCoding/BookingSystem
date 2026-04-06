using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BookingSystem.DTOs;

namespace BookingSystem.Services;

public class PetService
{
    private readonly BookingDbContext _context;
    private readonly string _imgbbApiKey;

    public PetService(BookingDbContext context)
    {
        _context = context;
        _imgbbApiKey = Environment.GetEnvironmentVariable("IMGBB_API_KEY")
                      ?? throw new InvalidOperationException("IMGBB_API_KEY chưa được cấu hình");
    }

    // ====================== UPLOAD ẢNH ======================
    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Không có file được upload");

        if (file.Length > 10 * 1024 * 1024)
            throw new ArgumentException("Ảnh phải nhỏ hơn 10MB");

        if (!file.ContentType.StartsWith("image/"))
            throw new ArgumentException("Chỉ được upload file ảnh");

        try
        {
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "image", file.FileName);

            var response = await client.PostAsync(
                $"https://api.imgbb.com/1/upload?key={_imgbbApiKey}", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var url = doc.RootElement
                         .GetProperty("data")
                         .GetProperty("url")
                         .GetString();

            if (string.IsNullOrEmpty(url))
                throw new InvalidOperationException("Không lấy được URL ảnh từ ImgBB");

            return url;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Upload ảnh thất bại: {ex.Message}");
        }
    }

    // ====================== LẤY DANH SÁCH PET CỦA USER ======================
    public async Task<List<object>> GetMyPetsAsync(Guid userId)
    {
        return await _context.Pets
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
            .ToListAsync<object>();
    }

    // ====================== LẤY DANH SÁCH LOÀI ======================
    public async Task<List<object>> GetSpeciesAsync()
    {
        return await _context.Species
            .Select(s => new { s.Id, s.Name })
            .ToListAsync<object>();
    }

    // ====================== LẤY DANH SÁCH GIỐNG THEO LOÀI ======================
    public async Task<List<object>> GetBreedsBySpeciesAsync(Guid speciesId)
    {
        return await _context.Breeds
            .Where(b => b.SpeciesId == speciesId)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync<object>();
    }

    // ====================== TẠO PET MỚI ======================
    public async Task<object> CreatePetAsync(PetRequest req, Guid userId)
    {
        var species = await _context.Species
            .FirstOrDefaultAsync(s => s.Name.ToLower() == req.Species.ToLower());

        if (species == null)
            throw new InvalidOperationException($"Không tìm thấy loài: {req.Species}");

        Guid? breedId = null;
        if (!string.IsNullOrWhiteSpace(req.Breed))
        {
            var breed = await _context.Breeds
                .FirstOrDefaultAsync(b => b.Name.ToLower() == req.Breed.ToLower()
                                       && b.SpeciesId == species.Id);

            if (breed == null)
                throw new InvalidOperationException($"Không tìm thấy giống '{req.Breed}' thuộc loài {req.Species}");

            breedId = breed.Id;
        }

        DateOnly? dob = null;
        if (!string.IsNullOrWhiteSpace(req.DateOfBirth)
            && DateOnly.TryParse(req.DateOfBirth, out var parsedDob))
        {
            dob = parsedDob;
        }

        var pet = new Pet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = req.Name.Trim(),
            SpeciesId = species.Id,
            BreedId = breedId,
            Gender = req.Gender?.Trim(),
            DateOfBirth = dob,
            Color = req.Color?.Trim(),
            Weight = req.Weight,
            Notes = req.Notes?.Trim(),
            ProfileImageUrl = req.ProfileImageUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        return new
        {
            pet.Id,
            pet.Name,
            Species = req.Species,
            Breed = req.Breed,
            pet.ProfileImageUrl
        };
    }

    // ====================== CẬP NHẬT PET (GIỮ ẢNH CŨ KHI KHÔNG CHỌN ẢNH MỚI) ======================
    public async Task<object> UpdatePetAsync(Guid petId, PetRequest req, Guid userId)
    {
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == petId && p.UserId == userId);

        if (pet == null)
            throw new InvalidOperationException("Không tìm thấy thú cưng hoặc bạn không có quyền chỉnh sửa");

        var species = await _context.Species
            .FirstOrDefaultAsync(s => s.Name.ToLower() == req.Species.ToLower());

        if (species == null)
            throw new InvalidOperationException($"Không tìm thấy loài: {req.Species}");

        Guid? breedId = null;
        if (!string.IsNullOrWhiteSpace(req.Breed))
        {
            var breed = await _context.Breeds
                .FirstOrDefaultAsync(b => b.Name.ToLower() == req.Breed.ToLower()
                                       && b.SpeciesId == species.Id);

            if (breed == null)
                throw new InvalidOperationException($"Không tìm thấy giống '{req.Breed}' thuộc loài {req.Species}");

            breedId = breed.Id;
        }

        DateOnly? dob = null;
        if (!string.IsNullOrWhiteSpace(req.DateOfBirth)
            && DateOnly.TryParse(req.DateOfBirth, out var parsedDob))
        {
            dob = parsedDob;
        }

        // Cập nhật thông tin
        pet.Name = req.Name.Trim();
        pet.SpeciesId = species.Id;
        pet.BreedId = breedId;
        pet.Gender = req.Gender?.Trim();
        pet.DateOfBirth = dob;
        pet.Color = req.Color?.Trim();
        pet.Weight = req.Weight;
        pet.Notes = req.Notes?.Trim();
        pet.UpdatedAt = DateTime.UtcNow;

        // Chỉ cập nhật ảnh khi có URL mới (không rỗng)
        if (!string.IsNullOrWhiteSpace(req.ProfileImageUrl))
        {
            pet.ProfileImageUrl = req.ProfileImageUrl.Trim();
        }

        await _context.SaveChangesAsync();

        return new
        {
            pet.Id,
            pet.Name,
            Species = req.Species,
            Breed = req.Breed,
            pet.ProfileImageUrl
        };
    }

    // ====================== LẤY TRẠNG THÁI BOOKING MỚI NHẤT CỦA PET ======================
    public async Task<object?> GetLatestBookingAsync(Guid petId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Store)
            .Where(b => b.PetId == petId)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (booking == null)
            return null;

        return new
        {
            storeName = booking.Store.Name,
            status = booking.Status,           // 0 = Pending, 1 = Cancelled, 2 = Completed...
            bookingDate = booking.BookingDate.ToString("yyyy-MM-dd")
        };
    }
}