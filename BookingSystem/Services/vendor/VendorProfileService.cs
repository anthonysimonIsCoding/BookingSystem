using BookingSystem.Data;
using BookingSystem.DTOs;           // Nếu bạn có DTO riêng thì dùng
using BookingSystem.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookingSystem.Services;

public class VendorProfileService
{
    private readonly BookingDbContext _context;
    private readonly string _imgbbApiKey;
    private readonly string _goongApiKey;
    private readonly string _goongTileKey;

    public VendorProfileService(BookingDbContext context)
    {
        _context = context;
        _imgbbApiKey = Environment.GetEnvironmentVariable("IMGBB_API_KEY") ?? "YOUR_KEY_HERE";
        _goongApiKey = Environment.GetEnvironmentVariable("GOONG_API_KEY") ?? "YOUR_KEY_HERE";
        _goongTileKey = Environment.GetEnvironmentVariable("GOONG_TILE_KEY") ?? "YOUR_KEY_HERE";
    }

    // ====================== LẤY THÔNG TIN CỬA HÀNG ======================
    public async Task<object> GetStoreInfoAsync(Guid userId)
    {
        var store = await _context.Stores
            .Include(s => s.Images)
            .Include(s => s.StoreCategories).ThenInclude(sc => sc.Category)
            .Include(s => s.StoreSpecies).ThenInclude(ss => ss.Species)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new InvalidOperationException("Không tìm thấy cửa hàng");

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

        return new
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
        };
    }

    // ====================== CẬP NHẬT THÔNG TIN CƠ BẢN ======================
    public async Task UpdateStoreInfoAsync(Guid userId, UpdateStoreRequest req)
    {
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        if (store == null)
            throw new InvalidOperationException("Không tìm thấy cửa hàng");

        store.Name = req.Name;
        store.Address = req.Address;
        store.Latitude = req.Latitude;
        store.Longitude = req.Longitude;
        store.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // ====================== UPLOAD ẢNH ======================
    public async Task<List<string>> UploadStoreImagesAsync(IFormFileCollection files)
    {
        if (files.Count == 0)
            throw new ArgumentException("Không có file nào được upload");

        var uploadedUrls = new List<string>();

        foreach (var file in files)
        {
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

                var response = await client.PostAsync($"https://api.imgbb.com/1/upload?key={_imgbbApiKey}", content);
                var json = await response.Content.ReadAsStringAsync();

                var url = JsonDocument.Parse(json)
                    .RootElement.GetProperty("data").GetProperty("url").GetString();

                if (!string.IsNullOrEmpty(url))
                    uploadedUrls.Add(url);
            }
            catch
            {
                throw new InvalidOperationException("Upload ảnh thất bại");
            }
        }

        return uploadedUrls;
    }

    // ====================== LƯU DANH SÁCH ẢNH ======================
    public async Task SaveStoreImagesAsync(Guid userId, List<StoreImageItem> images)
    {
        var store = await _context.Stores
            .Include(s => s.Images)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new InvalidOperationException("Không tìm thấy cửa hàng");

        // Xóa ảnh cũ
        _context.StoreImages.RemoveRange(store.Images);

        // Thêm ảnh mới
        foreach (var item in images)
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
    }

    // ====================== GOONG MAP PROXY ======================
    public string GetMapStyleUrl()
    {
        return $"https://tiles.goong.io/assets/goong_map_highlight.json?api_key={_goongTileKey}";
    }

    public async Task<string> MapAutocompleteAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            throw new ArgumentException("Input quá ngắn");

        using var client = new HttpClient();
        var url = $"https://rsapi.goong.io/place/autocomplete?api_key={_goongApiKey}&input={Uri.EscapeDataString(input)}";
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> MapPlaceDetailAsync(string placeId)
    {
        if (string.IsNullOrWhiteSpace(placeId))
            throw new ArgumentException("Thiếu place_id");

        using var client = new HttpClient();
        var url = $"https://rsapi.goong.io/place/detail?api_key={_goongApiKey}&place_id={placeId}";
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> MapReverseGeocodeAsync(decimal lat, decimal lng)
    {
        using var client = new HttpClient();
        var url = $"https://rsapi.goong.io/v2/geocode/street?api_key={_goongApiKey}&latlng={lat},{lng}";
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    // ====================== DANH MỤC & CHỦNG LOÀI ======================
    public async Task<object> GetCategoriesAndSpeciesAsync(Guid userId)
    {
        var store = await _context.Stores
            .Include(s => s.StoreCategories).ThenInclude(sc => sc.Category)
            .Include(s => s.StoreSpecies).ThenInclude(ss => ss.Species)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new InvalidOperationException("Không tìm thấy cửa hàng");

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

        return new { categories, species };
    }

    public async Task<object> GetAvailableCategoriesAndSpeciesAsync()
    {
        var categories = await _context.StoreCategories
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.Name, c.Description })
            .ToListAsync();

        var species = await _context.Species
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();

        return new { categories, species };
    }

    public async Task UpdateStoreCategoriesAsync(Guid userId, List<Guid> categoryIds)
    {
        var store = await _context.Stores
            .Include(s => s.StoreCategories)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new InvalidOperationException("Không tìm thấy cửa hàng");

        _context.StoreCategoryMappings.RemoveRange(store.StoreCategories);

        foreach (var catId in categoryIds)
        {
            store.StoreCategories.Add(new StoreCategoryMapping
            {
                StoreId = store.Id,
                CategoryId = catId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateStoreSpeciesAsync(Guid userId, List<Guid> speciesIds)
    {
        var store = await _context.Stores
            .Include(s => s.StoreSpecies)
            .FirstOrDefaultAsync(s => s.OwnerId == userId);

        if (store == null)
            throw new InvalidOperationException("Không tìm thấy cửa hàng");

        _context.StoreSpecies.RemoveRange(store.StoreSpecies);

        foreach (var spId in speciesIds)
        {
            store.StoreSpecies.Add(new StoreSpecies
            {
                StoreId = store.Id,
                SpeciesId = spId
            });
        }

        await _context.SaveChangesAsync();
    }
}