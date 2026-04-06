using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookingSystem.Entities.Enums;
using BookingSystem.Services;
using BookingSystem.DTOs;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly StoreService _storeService;

    public StoresController(StoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStores(
        string sort = "recommended",
        double? lat = null,
        double? lng = null,
        double radius = 300000,
        Guid? speciesId = null,
        string? categoryIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        decimal? minRating = null,
        string? search = null)
    {
        try
        {
            var result = await _storeService.GetStoresAsync(
                sort, lat, lng, radius, speciesId, categoryIds,
                minPrice, maxPrice, minRating, search);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var store = await _storeService.GetByIdAsync(id);

            if (store == null)
                return NotFound(new { message = "Store not found" });

            return Ok(store);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters()
    {
        try
        {
            var filters = await _storeService.GetFiltersAsync();
            return Ok(filters);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy filters", error = ex.Message });
        }
    }
}