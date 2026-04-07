using BookingSystem.DTOs;   // Nếu bạn di chuyển DTO ra đây
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Controllers.Vendor;

[Authorize(Roles = "ServiceProvider")]
[ApiController]
[Route("api/vendor/profile")]
public class VendorProfileController : ControllerBase
{
    private readonly VendorProfileService _vendorProfileService;

    public VendorProfileController(VendorProfileService vendorProfileService)
    {
        _vendorProfileService = vendorProfileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStoreInfo()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var data = await _vendorProfileService.GetStoreInfoAsync(userId);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _vendorProfileService.UpdateStoreInfoAsync(userId, req);
            return Ok(new { message = "Cập nhật thông tin thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("images")]
    public async Task<IActionResult> UploadStoreImages(IFormFileCollection files)
    {
        try
        {
            var urls = await _vendorProfileService.UploadStoreImagesAsync(files);
            return Ok(new { urls });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("images/save")]
    public async Task<IActionResult> SaveStoreImages([FromBody] SaveImagesRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _vendorProfileService.SaveStoreImagesAsync(userId, req.Images);
            return Ok(new { message = "Lưu danh sách ảnh thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // Goong Map endpoints
    [HttpGet("map/style")]
    public IActionResult GetMapStyle() => Ok(new { styleUrl = _vendorProfileService.GetMapStyleUrl() });

    [HttpGet("map/autocomplete")]
    public async Task<IActionResult> MapAutocomplete([FromQuery] string input)
    {
        try
        {
            var content = await _vendorProfileService.MapAutocompleteAsync(input);
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Tương tự cho các endpoint map/detail, map/reverse...

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategoriesAndSpecies()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var data = await _vendorProfileService.GetCategoriesAndSpeciesAsync(userId);
            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("available-categories")]
    public async Task<IActionResult> GetAvailableCategories()
    {
        var data = await _vendorProfileService.GetAvailableCategoriesAndSpeciesAsync();
        return Ok(data);
    }

    [HttpPut("categories")]
    public async Task<IActionResult> UpdateStoreCategories([FromBody] UpdateStoreCategoriesRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _vendorProfileService.UpdateStoreCategoriesAsync(userId, req.CategoryIds);
            return Ok(new { message = "Cập nhật danh mục thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("species")]
    public async Task<IActionResult> UpdateStoreSpecies([FromBody] UpdateStoreSpeciesRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _vendorProfileService.UpdateStoreSpeciesAsync(userId, req.SpeciesIds);
            return Ok(new { message = "Cập nhật chủng loài thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}