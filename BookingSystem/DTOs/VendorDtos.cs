using BookingSystem.Entities.Enums;
using System;
using System.Collections.Generic;

namespace BookingSystem.DTOs;

// ====================== VENDOR PROFILE DTOs ======================

public class UpdateStoreRequest
{
	public string Name { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public decimal? Latitude { get; set; }
	public decimal? Longitude { get; set; }
}

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

public class UpdateStoreCategoriesRequest
{
	public List<Guid> CategoryIds { get; set; } = new();
}

public class UpdateStoreSpeciesRequest
{
	public List<Guid> SpeciesIds { get; set; } = new();
}

// ====================== VENDOR ORDERS DTOs ======================

public class UpdateOrderStatusRequest
{
	public BookingStatus Status { get; set; }
}

public class VendorOrderDto
{
	public Guid Id { get; set; }
	public DateOnly BookingDate { get; set; }
	public string TimeSlot { get; set; } = string.Empty;   // "09:00 - 10:00"
	public string StartTime { get; set; } = string.Empty;  // Thêm vào
	public string EndTime { get; set; } = string.Empty;    // Thêm vào
	public string CustomerName { get; set; } = string.Empty;
	public string PetName { get; set; } = string.Empty;
	public string ServiceName { get; set; } = string.Empty;
	public decimal TotalPrice { get; set; }
	public BookingStatus Status { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class VendorOrderDetailDto
{
	public Guid Id { get; set; }
	public DateOnly BookingDate { get; set; }
	public string StartTime { get; set; } = string.Empty;
	public string EndTime { get; set; } = string.Empty;

	public string CustomerName { get; set; } = string.Empty;
	public string CustomerPhone { get; set; } = string.Empty;

	// Pet Information
	public string PetName { get; set; } = string.Empty;
	public string PetSpecies { get; set; } = string.Empty;
	public string? PetBreed { get; set; }
	public string? PetGender { get; set; }
	public string? PetDateOfBirth { get; set; }
	public string? PetColor { get; set; }
	public double? PetWeight { get; set; }
	public string? PetNotes { get; set; }
	public string? PetProfileImageUrl { get; set; }

	// Service Information
	public string MainServiceName { get; set; } = string.Empty;
	public List<OrderOptionDto> Options { get; set; } = new();   // Dùng List<OrderOptionDto>

	public decimal TotalPrice { get; set; }
	public BookingStatus Status { get; set; }
	public string? Notes { get; set; }

	// Voucher
	public string? PlatformVoucherCode { get; set; }
	public string? StoreVoucherCode { get; set; }
	public decimal? PlatformDiscount { get; set; }
	public decimal? StoreDiscount { get; set; }

	public DateTime CreatedAt { get; set; }
}

// DTO nhỏ cho Options
public class OrderOptionDto
{
	public string OptionName { get; set; } = string.Empty;
	public decimal Price { get; set; }
}

public class VendorCalendarOrderDto
{
	public Guid Id { get; set; }
	public DateOnly BookingDate { get; set; }
	public string TimeSlot { get; set; } = string.Empty;
	public string CustomerName { get; set; } = string.Empty;
	public string PetName { get; set; } = string.Empty;
	public string ServiceName { get; set; } = string.Empty;
	public BookingStatus Status { get; set; }
}

// ====================== VENDOR SERVICES DTOs ======================

public class ServiceCreateDto
{
	public string Name { get; set; } = null!;
	public string? Description { get; set; }
	public decimal Price { get; set; }
	public int DurationMinutes { get; set; }
	public int Type { get; set; }           // 0 = Single, 1 = Multiple
	public bool IsActive { get; set; } = true;
	public List<OptionGroupDto> OptionGroups { get; set; } = new();
}

public class OptionGroupDto
{
	public string? Id { get; set; }
	public string Name { get; set; } = null!;
	public int Type { get; set; }           // 0 = SingleChoice, 1 = MultiChoice
	public bool IsRequired { get; set; }
	public List<OptionDto> Options { get; set; } = new();
}

public class OptionDto
{
	public string? Id { get; set; }
	public string Name { get; set; } = null!;
	public decimal Price { get; set; }
	public int DurationMinutes { get; set; }
	public bool IsActive { get; set; } = true;
}

// ====================== VENDOR VOUCHER DTOs ======================

public class StoreVoucherCreateDto
{
	public string Code { get; set; } = null!;
	public string? Name { get; set; }
	public string? Description { get; set; }

	public int DiscountType { get; set; }
	public decimal DiscountValue { get; set; }

	public decimal? MinOrderValue { get; set; }
	public decimal? MaxDiscountAmount { get; set; }

	public int? UsageLimitPerUser { get; set; }
	public int? TotalUsageLimit { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public bool IsActive { get; set; } = true;

	public Guid? ApplicableServiceId { get; set; }
	public Guid? ApplicableSpeciesId { get; set; }
}

// ====================== VENDOR TIMESLOT DTOs ======================

public class TimeSlotRequest
{
	public string? Id { get; set; }
	public TimeSpan StartTime { get; set; }
	public TimeSpan EndTime { get; set; }
	public int Capacity { get; set; }
	public bool IsActive { get; set; }
}

public class TimeSlotOverrideRequest
{
	public Guid? TimeSlotId { get; set; }
	public DateOnly Date { get; set; }
	public TimeSpan? StartTime { get; set; }
	public TimeSpan? EndTime { get; set; }
	public int? Capacity { get; set; }
	public bool IsFullDayClosure { get; set; } = false;
	public string? Reason { get; set; }
}