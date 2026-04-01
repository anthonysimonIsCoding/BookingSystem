// Controllers/Vendor/VendorProfileController.cs
using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor/profile")]
public class VendorProfileController : ControllerBase
{
    private readonly BookingDbContext _context;
    private readonly string _imgbbApiKey;
    private readonly string _goongApiKey;
    private readonly string _goongTileKey;

    public VendorProfileController(BookingDbContext context)
    {
        _context = context;
        _imgbbApiKey = Environment.GetEnvironmentVariable("IMGBB_API_KEY") ?? "YOUR_KEY_HERE";
        _goongApiKey = Environment.GetEnvironmentVariable("GOONG_API_KEY") ?? "YOUR_KEY_HERE";
        _goongTileKey = Environment.GetEnvironmentVariable("GOONG_TILE_KEY") ?? "YOUR_KEY_HERE";
    }

    // ====================== LẤY THÔNG TIN CỬA HÀNG ======================
    [HttpGet]
    public async Task<IActionResult> GetStoreInfo()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var store = await _context.Stores
            .Include(s => s.Images)
            .Include(s => s.StoreCategories)
                .ThenInclude(sc => sc.Category)
            .Include(s => s.StoreSpecies)
                .ThenInclude(ss => ss.Species)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        var images = store.Images
            .OrderBy(i => i.Order)
            .Select(i => new
            {
                i.Id,
                i.ImageUrl,
                i.IsThumbnail,
                i.Order
            })
            .ToList();

        var categories = store.StoreCategories
            .Select(sc => new
            {
                categoryId = sc.CategoryId,
                name = sc.Category.Name
            })
            .ToList();

        var species = store.StoreSpecies
            .Select(ss => new
            {
                speciesId = ss.SpeciesId,
                name = ss.Species.Name
            })
            .ToList();

        return Ok(new
        {
            store.Id,
            store.Name,
            store.Address,
            store.Latitude,
            store.Longitude,
            store.AverageRating,
            store.ReviewCount,
            images,
            categories,
            species
        });
    }

    // ====================== CẬP NHẬT THÔNG TIN CƠ BẢN ======================
    public class UpdateStoreRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        store.Name = req.Name;
        store.Address = req.Address;
        store.Latitude = req.Latitude;
        store.Longitude = req.Longitude;
        store.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật thông tin thành công" });
    }

    // ====================== UPLOAD ẢNH ======================
    [HttpPost("images")]
    public async Task<IActionResult> UploadStoreImages(IFormFileCollection files)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        if (files.Count == 0) return BadRequest("Không có file");

        var uploadedUrls = new List<string>();

        foreach (var file in files)
        {
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

                uploadedUrls.Add(url);
            }
            catch
            {
                return BadRequest("Upload ảnh thất bại");
            }
        }

        return Ok(new { urls = uploadedUrls });
    }

    // ====================== LƯU DANH SÁCH ẢNH ======================
    public class SaveImagesRequest
    {
        public List<StoreImageItem> Images { get; set; } = new();
    }

    public class StoreImageItem
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsThumbnail { get; set; }
        public int Order { get; set; }
    }

    [HttpPost("images/save")]
    public async Task<IActionResult> SaveStoreImages([FromBody] SaveImagesRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var store = await _context.Stores
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        _context.StoreImages.RemoveRange(store.Images);

        foreach (var item in req.Images)
        {
            var img = new StoreImage
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                ImageUrl = item.ImageUrl,
                IsThumbnail = item.IsThumbnail,
                Order = item.Order,
                CreatedAt = DateTime.UtcNow
            };
            _context.StoreImages.Add(img);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Lưu danh sách ảnh thành công" });
    }

    // ====================== PROXY GOONG MAP ======================

    [HttpGet("map/style")]
    public IActionResult GetMapStyle()
    {
        var styleUrl = $"https://tiles.goong.io/assets/goong_map_highlight.json?api_key={_goongTileKey}";
        return Ok(new { styleUrl });
    }

    [HttpGet("map/autocomplete")]
    public async Task<IActionResult> MapAutocomplete([FromQuery] string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            return BadRequest("Input quá ngắn");

        try
        {
            using var client = new HttpClient();
            var url = $"https://rsapi.goong.io/place/autocomplete?api_key={_goongApiKey}&input={Uri.EscapeDataString(input)}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi gọi Goong API", error = ex.Message });
        }
    }

    [HttpGet("map/detail")]
    public async Task<IActionResult> MapPlaceDetail([FromQuery] string place_id)
    {
        if (string.IsNullOrWhiteSpace(place_id))
            return BadRequest("Thiếu place_id");

        try
        {
            using var client = new HttpClient();
            var url = $"https://rsapi.goong.io/place/detail?api_key={_goongApiKey}&place_id={place_id}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi gọi Goong API", error = ex.Message });
        }
    }

    [HttpGet("map/reverse")]
    public async Task<IActionResult> MapReverseGeocode([FromQuery] decimal lat, [FromQuery] decimal lng)
    {
        try
        {
            using var client = new HttpClient();
            var url = $"https://rsapi.goong.io/v2/geocode/street?api_key={_goongApiKey}&latlng={lat},{lng}";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi gọi Goong API", error = ex.Message });
        }
    }
    // ====================== DANH MỤC & CHỦNG LOÀI ======================
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategoriesAndSpecies()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var store = await _context.Stores
            .Include(s => s.StoreCategories).ThenInclude(sc => sc.Category)
            .Include(s => s.StoreSpecies).ThenInclude(ss => ss.Species)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        var categories = store.StoreCategories.Select(sc => new
        {
            categoryId = sc.CategoryId,
            name = sc.Category.Name
        }).ToList();

        var species = store.StoreSpecies.Select(ss => new
        {
            speciesId = ss.SpeciesId,
            name = ss.Species.Name
        }).ToList();

        return Ok(new { categories, species });
    }

    [HttpGet("available-categories")]
    public async Task<IActionResult> GetAvailableCategories()
    {
        var categories = await _context.StoreCategories
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.Name, c.Description })
            .ToListAsync();

        var allSpecies = await _context.Species
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();

        return Ok(new { categories, species = allSpecies });
    }

    [HttpPut("categories")]
    public async Task<IActionResult> UpdateStoreCategories([FromBody] UpdateStoreCategoriesRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.Include(s => s.StoreCategories).FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        _context.StoreCategoryMappings.RemoveRange(store.StoreCategories);

        foreach (var catId in req.CategoryIds)
        {
            store.StoreCategories.Add(new StoreCategoryMapping
            {
                StoreId = store.Id,
                CategoryId = catId
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật danh mục thành công" });
    }

    [HttpPut("species")]
    public async Task<IActionResult> UpdateStoreSpecies([FromBody] UpdateStoreSpeciesRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var store = await _context.Stores.Include(s => s.StoreSpecies).FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null) return NotFound("Không tìm thấy cửa hàng");

        _context.StoreSpecies.RemoveRange(store.StoreSpecies);

        foreach (var spId in req.SpeciesIds)
        {
            store.StoreSpecies.Add(new StoreSpecies
            {
                StoreId = store.Id,
                SpeciesId = spId
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Cập nhật chủng loài thành công" });
    }

    public class UpdateStoreCategoriesRequest
    {
        public List<Guid> CategoryIds { get; set; } = new();
    }

    public class UpdateStoreSpeciesRequest
    {
        public List<Guid> SpeciesIds { get; set; } = new();
    }
}