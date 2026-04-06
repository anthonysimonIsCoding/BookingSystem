using System;
namespace BookingSystem.DTOs;

public class PetRequest
{
	public string Name { get; set; } = null!;
	public string Species { get; set; } = null!;
	public string? Breed { get; set; }
	public string? Gender { get; set; }
	public string? DateOfBirth { get; set; }     // Ví dụ: "2023-05-15"
	public string? Color { get; set; }
	public double? Weight { get; set; }
	public string? Notes { get; set; }
	public string? ProfileImageUrl { get; set; }
}