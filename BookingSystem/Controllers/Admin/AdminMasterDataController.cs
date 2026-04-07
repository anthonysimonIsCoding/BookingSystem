using BookingSystem.DTOs;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingSystem.Entities;

namespace BookingSystem.Controllers.Admin;

[ApiController]
[Route("api/admin/masterdata")]
//[Authorize(Roles = "Admin")]   // Bỏ comment khi đã có Auth Admin
public class AdminMasterDataController : ControllerBase
{
    private readonly AdminMasterDataService _adminMasterDataService;

    public AdminMasterDataController(AdminMasterDataService adminMasterDataService)
    {
        _adminMasterDataService = adminMasterDataService;
    }

    #region ==================== STORE CATEGORY ====================

    [HttpGet("store-categories")]
    public async Task<IActionResult> GetStoreCategories()
    {
        var list = await _adminMasterDataService.GetStoreCategoriesAsync();
        return Ok(list);
    }

    [HttpPost("store-categories")]
    public async Task<IActionResult> CreateStoreCategory([FromBody] StoreCategory category)
    {
        try
        {
            var result = await _adminMasterDataService.CreateStoreCategoryAsync(category);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo danh mục", error = ex.Message });
        }
    }

    [HttpPut("store-categories/{id}")]
    public async Task<IActionResult> UpdateStoreCategory(Guid id, [FromBody] StoreCategory category)
    {
        try
        {
            var result = await _adminMasterDataService.UpdateStoreCategoryAsync(id, category);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy danh mục");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật", error = ex.Message });
        }
    }

    [HttpDelete("store-categories/{id}")]
    public async Task<IActionResult> DeleteStoreCategory(Guid id)
    {
        try
        {
            await _adminMasterDataService.DeleteStoreCategoryAsync(id);
            return Ok(new { message = "Đã xóa danh mục" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy danh mục");
        }
    }

    #endregion

    #region ==================== SPECIES ====================

    [HttpGet("species")]
    public async Task<IActionResult> GetSpecies()
    {
        var list = await _adminMasterDataService.GetSpeciesAsync();
        return Ok(list);
    }

    [HttpPost("species")]
    public async Task<IActionResult> CreateSpecies([FromBody] Species species)
    {
        try
        {
            var result = await _adminMasterDataService.CreateSpeciesAsync(species);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo loài", error = ex.Message });
        }
    }

    [HttpPut("species/{id}")]
    public async Task<IActionResult> UpdateSpecies(Guid id, [FromBody] Species species)
    {
        try
        {
            var result = await _adminMasterDataService.UpdateSpeciesAsync(id, species);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy loài");
        }
    }

    [HttpDelete("species/{id}")]
    public async Task<IActionResult> DeleteSpecies(Guid id)
    {
        try
        {
            await _adminMasterDataService.DeleteSpeciesAsync(id);
            return Ok(new { message = "Đã xóa loài" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy loài");
        }
    }

    #endregion

    #region ==================== BREED ====================

    [HttpGet("breeds")]
    public async Task<IActionResult> GetBreeds()
    {
        var result = await _adminMasterDataService.GetBreedsAsync();
        return Ok(result);
    }

    [HttpPost("breeds")]
    public async Task<IActionResult> CreateBreed([FromBody] BreedCreateDto dto)
    {
        try
        {
            var result = await _adminMasterDataService.CreateBreedAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi tạo giống", error = ex.Message });
        }
    }

    [HttpPut("breeds/{id}")]
    public async Task<IActionResult> UpdateBreed(Guid id, [FromBody] BreedCreateDto dto)
    {
        try
        {
            var result = await _adminMasterDataService.UpdateBreedAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy giống");
        }
    }

    [HttpDelete("breeds/{id}")]
    public async Task<IActionResult> DeleteBreed(Guid id)
    {
        try
        {
            await _adminMasterDataService.DeleteBreedAsync(id);
            return Ok(new { message = "Đã xóa giống" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Không tìm thấy giống");
        }
    }

    #endregion
}