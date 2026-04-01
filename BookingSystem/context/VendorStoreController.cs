//using BookingSystem.Data;
//using BookingSystem.Entities;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text.Json;
//using System.Threading.Tasks;


//namespace BookingSystem.Controllers.Vendor;

//[Authorize(Roles = "ServiceProvider")]
//[ApiController]
//[Route("api/vendor/store")]
//public class VendorStoreController : ControllerBase
//{
//    private readonly BookingDbContext _context;
//    private readonly string _imgbbApiKey;

//    public VendorStoreController(BookingDbContext context)
//    {
//        _context = context;
//        _imgbbApiKey = Environment.GetEnvironmentVariable("IMGBB_API_KEY") ?? "YOUR_KEY_HERE";

//    }

//    // ====================== LẤY THÔNG TIN CỬA HÀNG ======================
//    // ====================== LẤY THÔNG TIN CỬA HÀNG (ĐÃ SỬA) ======================
//    [HttpGet]
//    public async Task<IActionResult> GetStoreInfo()
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//        var store = await _context.Stores
//            .Include(s => s.Images)
//            .Include(s => s.StoreCategories)
//                .ThenInclude(sc => sc.Category)
//            .Include(s => s.StoreSpecies)
//                .ThenInclude(ss => ss.Species)
//            .FirstOrDefaultAsync(s => s.OwnerId == userId);

//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        var images = store.Images
//            .OrderBy(i => i.Order)
//            .Select(i => new
//            {
//                i.Id,
//                i.ImageUrl,
//                i.IsThumbnail,
//                i.Order
//            })
//            .ToList();

//        var categories = store.StoreCategories
//            .Select(sc => new
//            {
//                categoryId = sc.CategoryId,
//                name = sc.Category.Name
//            })
//            .ToList();

//        var species = store.StoreSpecies
//            .Select(ss => new
//            {
//                speciesId = ss.SpeciesId,
//                name = ss.Species.Name
//            })
//            .ToList();

//        return Ok(new
//        {
//            store.Id,
//            store.Name,
//            store.Address,
//            store.Latitude,
//            store.Longitude,
//            store.AverageRating,
//            store.ReviewCount,
//            images,
//            categories,     // ← Quan trọng
//            species         // ← Quan trọng
//        });
//    }

//    // ====================== CẬP NHẬT THÔNG TIN CƠ BẢN ======================
//    public class UpdateStoreRequest
//    {
//        public string Name { get; set; } = string.Empty;
//        public string Address { get; set; } = string.Empty;
//        public decimal? Latitude { get; set; }
//        public decimal? Longitude { get; set; }
//    }

//    [HttpPut]
//    public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreRequest req)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        store.Name = req.Name;
//        store.Address = req.Address;
//        store.Latitude = req.Latitude;
//        store.Longitude = req.Longitude;
//        store.UpdatedAt = DateTime.UtcNow;

//        await _context.SaveChangesAsync();
//        return Ok(new { message = "Cập nhật thông tin thành công" });
//    }

//    // ====================== UPLOAD ẢNH LÊN IMGBB ======================
//    [HttpPost("images")]
//    public async Task<IActionResult> UploadStoreImages(IFormFileCollection files)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        if (files.Count == 0) return BadRequest("Không có file");

//        var uploadedUrls = new List<string>();

//        foreach (var file in files)
//        {
//            if (file.Length > 10 * 1024 * 1024) return BadRequest("Ảnh phải < 10MB");
//            if (!file.ContentType.StartsWith("image/")) return BadRequest("Chỉ upload ảnh");

//            try
//            {
//                using var client = new HttpClient();
//                using var content = new MultipartFormDataContent();
//                using var stream = file.OpenReadStream();
//                var fileContent = new StreamContent(stream);
//                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
//                content.Add(fileContent, "image", file.FileName);

//                var response = await client.PostAsync($"https://api.imgbb.com/1/upload?key={_imgbbApiKey}", content);
//                var json = await response.Content.ReadAsStringAsync();
//                var url = JsonDocument.Parse(json).RootElement.GetProperty("data").GetProperty("url").GetString();
//                uploadedUrls.Add(url);
//            }
//            catch { return BadRequest("Upload ảnh thất bại"); }
//        }

//        return Ok(new { urls = uploadedUrls });
//    }

//    // ====================== LƯU DANH SÁCH ẢNH (Order + IsThumbnail) ======================
//    public class SaveImagesRequest
//    {
//        public List<StoreImageItem> Images { get; set; } = new();
//    }

//    public class StoreImageItem
//    {
//        public string ImageUrl { get; set; } = string.Empty;
//        public bool IsThumbnail { get; set; }
//        public int Order { get; set; }
//    }

//    // ====================== LƯU DANH SÁCH ẢNH (Order + IsThumbnail) ======================
//    [HttpPost("images/save")]
//    public async Task<IActionResult> SaveStoreImages([FromBody] SaveImagesRequest req)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

//        // Lấy store và load luôn tất cả ảnh cũ
//        var store = await _context.Stores
//            .Include(s => s.Images)
//            .FirstOrDefaultAsync(s => s.OwnerId == userId);

//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        // XÓA HẾT ảnh cũ của cửa hàng này
//        _context.StoreImages.RemoveRange(store.Images);

//        // Thêm danh sách ảnh mới (với thứ tự và IsThumbnail đúng)
//        foreach (var item in req.Images)
//        {
//            var img = new StoreImage
//            {
//                Id = Guid.NewGuid(),
//                StoreId = store.Id,
//                ImageUrl = item.ImageUrl,
//                IsThumbnail = item.IsThumbnail,
//                Order = item.Order,
//                CreatedAt = DateTime.UtcNow
//            };
//            _context.StoreImages.Add(img);
//        }

//        await _context.SaveChangesAsync();

//        return Ok(new { message = "Lưu danh sách ảnh thành công" });
//    }

//    // ====================== LẤY DANH MỤC & CHỦNG LOÀI ======================
//    [HttpGet("categories")]
//    public async Task<IActionResult> GetCategoriesAndSpecies()
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var store = await _context.Stores
//            .Include(s => s.StoreCategories)
//                .ThenInclude(sc => sc.Category)
//            .Include(s => s.StoreSpecies)
//                .ThenInclude(ss => ss.Species)
//            .FirstOrDefaultAsync(s => s.OwnerId == userId);

//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        var categories = store.StoreCategories
//            .Select(sc => new
//            {
//                categoryId = sc.CategoryId,
//                name = sc.Category.Name,
//                description = sc.Category.Description
//            }).ToList();

//        var species = store.StoreSpecies
//            .Select(ss => new
//            {
//                speciesId = ss.SpeciesId,
//                name = ss.Species.Name
//            }).ToList();

//        return Ok(new
//        {
//            categories,
//            species
//        });
//    }

//    // ====================== LẤY TẤT CẢ DANH MỤC & CHỦNG LOÀI (để chọn) ======================
//    [HttpGet("available-categories")]
//    public async Task<IActionResult> GetAvailableCategories()
//    {
//        var categories = await _context.StoreCategories
//            .Where(c => c.IsActive)
//            .Select(c => new { c.Id, c.Name, c.Description })
//            .ToListAsync();

//        var allSpecies = await _context.Species
//            .Select(s => new { s.Id, s.Name })
//            .ToListAsync();

//        return Ok(new { categories, species = allSpecies });
//    }

//    // ====================== CẬP NHẬT DANH MỤC CỦA STORE ======================
//    [HttpPut("categories")]
//    public async Task<IActionResult> UpdateStoreCategories([FromBody] UpdateStoreCategoriesRequest req)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var store = await _context.Stores
//            .Include(s => s.StoreCategories)
//            .FirstOrDefaultAsync(s => s.OwnerId == userId);

//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        // Xóa hết mapping cũ
//        _context.StoreCategoryMappings.RemoveRange(store.StoreCategories);

//        // Thêm mapping mới
//        foreach (var catId in req.CategoryIds)
//        {
//            store.StoreCategories.Add(new StoreCategoryMapping
//            {
//                StoreId = store.Id,
//                CategoryId = catId
//            });
//        }

//        await _context.SaveChangesAsync();
//        return Ok(new { message = "Cập nhật danh mục thành công" });
//    }

//    // ====================== CẬP NHẬT CHỦNG LOÀI CỦA STORE ======================
//    [HttpPut("species")]
//    public async Task<IActionResult> UpdateStoreSpecies([FromBody] UpdateStoreSpeciesRequest req)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var store = await _context.Stores
//            .Include(s => s.StoreSpecies)
//            .FirstOrDefaultAsync(s => s.OwnerId == userId);

//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        // Xóa hết mapping cũ
//        _context.StoreSpecies.RemoveRange(store.StoreSpecies);

//        // Thêm mapping mới
//        foreach (var spId in req.SpeciesIds)
//        {
//            store.StoreSpecies.Add(new StoreSpecies
//            {
//                StoreId = store.Id,
//                SpeciesId = spId
//            });
//        }

//        await _context.SaveChangesAsync();
//        return Ok(new { message = "Cập nhật chủng loài thành công" });
//    }

//    public class UpdateStoreCategoriesRequest
//    {
//        public List<Guid> CategoryIds { get; set; } = new();
//    }

//    public class UpdateStoreSpeciesRequest
//    {
//        public List<Guid> SpeciesIds { get; set; } = new();
//    }
//    // ====================== LẤY TIMESLOT CỐ ĐỊNH ======================
//    [HttpGet("timeslots")]
//    public async Task<IActionResult> GetTimeSlots()
//    {
//        try
//        {
//            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
//            if (store == null) return NotFound("Không tìm thấy cửa hàng");

//            var slots = await _context.TimeSlots
//                .Where(t => t.StoreId == store.Id)
//                .Select(t => new
//                {
//                    t.Id,
//                    t.StartTime,
//                    t.EndTime,
//                    t.Capacity,
//                    t.IsActive
//                })
//                .OrderBy(t => t.StartTime)
//                .ToListAsync();

//            return Ok(slots);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error in GetTimeSlots: {ex.Message}");
//            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
//        }
//    }

//    // ====================== LƯU / THÊM / SỬA TIMESLOT CỐ ĐỊNH ======================
//    [HttpPost("timeslots")]
//    public async Task<IActionResult> SaveTimeSlots([FromBody] List<TimeSlotRequest> requests)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        foreach (var req in requests)
//        {
//            // Kiểm tra giờ kết thúc phải lớn hơn giờ bắt đầu
//            if (req.StartTime >= req.EndTime)
//                return BadRequest($"Khung giờ {req.StartTime} - {req.EndTime} không hợp lệ. Giờ kết thúc phải lớn hơn giờ bắt đầu.");

//            if (!string.IsNullOrEmpty(req.Id) && Guid.TryParse(req.Id, out Guid reqId))
//            {
//                // EDIT
//                var existing = await _context.TimeSlots.FirstOrDefaultAsync(t => t.Id == reqId && t.StoreId == store.Id);
//                if (existing != null)
//                {
//                    existing.StartTime = req.StartTime;
//                    existing.EndTime = req.EndTime;
//                    existing.Capacity = req.Capacity;
//                    existing.IsActive = req.IsActive;
//                    existing.UpdatedAt = DateTime.UtcNow;
//                }
//            }
//            else
//            {
//                // THÊM MỚI
//                var isDuplicate = await _context.TimeSlots.AnyAsync(t =>
//                    t.StoreId == store.Id &&
//                    t.StartTime == req.StartTime &&
//                    t.EndTime == req.EndTime);

//                if (isDuplicate)
//                    return BadRequest($"Khung giờ {req.StartTime} - {req.EndTime} đã tồn tại");

//                var newSlot = new TimeSlot
//                {
//                    Id = Guid.NewGuid(),
//                    StoreId = store.Id,
//                    StartTime = req.StartTime,
//                    EndTime = req.EndTime,
//                    Capacity = req.Capacity,
//                    IsActive = req.IsActive,
//                    CreatedAt = DateTime.UtcNow
//                };
//                _context.TimeSlots.Add(newSlot);
//            }
//        }

//        await _context.SaveChangesAsync();
//        return Ok(new { message = "Lưu timeslot thành công" });
//    }

//    public class TimeSlotRequest
//    {
//        public string? Id { get; set; }
//        public TimeSpan StartTime { get; set; }
//        public TimeSpan EndTime { get; set; }
//        public int Capacity { get; set; }
//        public bool IsActive { get; set; }
//    }
//    // ====================== OVERRIDE TIMESLOT (đầy đủ) ======================
//    [HttpGet("overrides")]
//    public async Task<IActionResult> GetOverrides([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
//    {
//        try
//        {
//            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//            var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
//            if (store == null) return NotFound("Không tìm thấy cửa hàng");

//            var query = _context.TimeSlotOverrides.Where(o => o.StoreId == store.Id);

//            if (fromDate.HasValue) query = query.Where(o => o.Date >= fromDate.Value);
//            if (toDate.HasValue) query = query.Where(o => o.Date <= toDate.Value);

//            var data = await query
//                .OrderByDescending(o => o.Date)
//                .ThenBy(o => o.StartTime)
//                .ToListAsync();

//            return Ok(data);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Error in GetOverrides: {ex.Message}");
//            return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
//        }
//    }

//    [HttpPost("overrides")]
//    public async Task<IActionResult> CreateOverride([FromBody] TimeSlotOverrideRequest req)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var store = await _context.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
//        if (store == null) return NotFound("Không tìm thấy cửa hàng");

//        // Kiểm tra ngày
//        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
//        var maxDate = today.AddMonths(6);

//        if (req.Date < today) return BadRequest("Không thể tạo override cho ngày trong quá khứ");
//        if (req.Date > maxDate) return BadRequest("Chỉ được tạo override tối đa 6 tháng tới");

//        // Kiểm tra giờ nếu không phải nghỉ cả ngày
//        if (!req.IsFullDayClosure && req.StartTime.HasValue && req.EndTime.HasValue)
//        {
//            if (req.StartTime >= req.EndTime)
//                return BadRequest("Giờ kết thúc phải lớn hơn giờ bắt đầu");
//        }

//        var ov = new TimeSlotOverride
//        {
//            StoreId = store.Id,
//            TimeSlotId = req.TimeSlotId,
//            Date = req.Date,
//            StartTime = req.IsFullDayClosure ? null : req.StartTime,
//            EndTime = req.IsFullDayClosure ? null : req.EndTime,
//            Capacity = req.IsFullDayClosure ? null : req.Capacity,
//            IsFullDayClosure = req.IsFullDayClosure,
//            Reason = req.Reason ?? "Override thủ công",
//            CreatedByUserId = userId,
//            CreatedAt = DateTime.UtcNow,
//            IsActive = true
//        };

//        _context.TimeSlotOverrides.Add(ov);
//        await _context.SaveChangesAsync();

//        return Ok(new { message = "Tạo override thành công" });
//    }

//    [HttpPut("overrides/{id}")]
//    public async Task<IActionResult> UpdateOverride(Guid id, [FromBody] TimeSlotOverrideRequest req)
//    {
//        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
//        var ov = await _context.TimeSlotOverrides
//            .FirstOrDefaultAsync(o => o.Id == id && o.StoreId == _context.Stores.First(s => s.OwnerId == userId).Id);

//        if (ov == null) return NotFound("Không tìm thấy override");

//        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
//        var maxDate = today.AddMonths(6);

//        if (req.Date < today) return BadRequest("Không thể đặt override cho ngày trong quá khứ");
//        if (req.Date > maxDate) return BadRequest("Chỉ được tạo override tối đa 6 tháng tới");

//        if (!req.IsFullDayClosure && req.StartTime.HasValue && req.EndTime.HasValue)
//        {
//            if (req.StartTime >= req.EndTime)
//                return BadRequest("Giờ kết thúc phải lớn hơn giờ bắt đầu");
//        }

//        ov.Date = req.Date;
//        ov.TimeSlotId = req.TimeSlotId;
//        ov.StartTime = req.IsFullDayClosure ? null : req.StartTime;
//        ov.EndTime = req.IsFullDayClosure ? null : req.EndTime;
//        ov.Capacity = req.IsFullDayClosure ? null : req.Capacity;
//        ov.IsFullDayClosure = req.IsFullDayClosure;
//        ov.Reason = req.Reason;

//        await _context.SaveChangesAsync();
//        return Ok(new { message = "Cập nhật override thành công" });
//    }

//    public class TimeSlotOverrideRequest
//    {
//        public Guid? TimeSlotId { get; set; }
//        public DateOnly Date { get; set; }
//        public TimeSpan? StartTime { get; set; }
//        public TimeSpan? EndTime { get; set; }
//        public int? Capacity { get; set; }
//        public bool IsFullDayClosure { get; set; } = false;
//        public string? Reason { get; set; }
//    }
//}