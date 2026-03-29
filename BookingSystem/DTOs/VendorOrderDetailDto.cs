using System;
using System.Collections.Generic;
using BookingSystem.Entities.Enums;

namespace BookingSystem.DTOs;


public class VendorOrderDetailDto
{
	public Guid Id { get; set; }
	public DateOnly BookingDate { get; set; }
	public string StartTime { get; set; } = string.Empty;
	public string EndTime { get; set; } = string.Empty;
	public BookingStatus Status { get; set; }
	public decimal TotalPrice { get; set; }
	public string? Notes { get; set; }

	public string PetName { get; set; } = string.Empty;
	public string PetSpecies { get; set; } = string.Empty;
	public string? PetBreed { get; set; }
	public string? PetGender { get; set; }
	public string? PetDateOfBirth { get; set; }
	public string? PetColor { get; set; }
	public double? PetWeight { get; set; }
	public string? PetNotes { get; set; }
	public string? PetProfileImageUrl { get; set; }

	public string CustomerName { get; set; } = string.Empty;

	public string MainServiceName { get; set; } = string.Empty;
	public List<OrderOptionDto> Options { get; set; } = new();

	public string? PlatformVoucherCode { get; set; }
	public string? StoreVoucherCode { get; set; }
	public decimal? PlatformDiscount { get; set; }
	public decimal? StoreDiscount { get; set; }
}