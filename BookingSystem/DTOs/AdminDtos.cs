using System;
using BookingSystem.Entities.Enums;
namespace BookingSystem.DTOs;

public class BreedCreateDto
{
    public string Name { get; set; } = null!;
    public Guid SpeciesId { get; set; }
}

// ====================== ADMIN PLATFORM VOUCHER DTO ======================

public class PlatformVoucherCreateDto
{
    public string Code { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }

    public VoucherDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimitPerUser { get; set; }
    public int? TotalUsageLimit { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

// ====================== ADMIN STORE DTOs ======================

public class UpdateStoreStatusRequest
{
    public StoreStatus Status { get; set; }
}

public class ToggleServiceStatusRequest
{
    public bool IsActive { get; set; }
}

// ====================== ADMIN USER DTOs ======================

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}