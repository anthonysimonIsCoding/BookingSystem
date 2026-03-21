using BookingSystem.Data;
using BookingSystem.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	private readonly BookingDbContext _context;

	public UsersController(BookingDbContext context)
	{
		_context = context;
	}

	// GET api/users/me
	[HttpGet("me")]
	public async Task<IActionResult> GetProfile()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (userId == null)
			return Unauthorized();

		var user = await _context.Users
			.Where(u => u.Id == Guid.Parse(userId))
			.Select(u => new
			{
				u.Id,
				u.FullName,
				u.Email,
				u.PhoneNumber
			})
			.FirstOrDefaultAsync();

		if (user == null)
			return NotFound();

		return Ok(user);
	}

	// PUT api/users/me
	[HttpPut("me")]
	public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (userId == null)
			return Unauthorized();

		var user = await _context.Users
			.FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

		if (user == null)
			return NotFound();

		user.FullName = request.FullName;
		user.PhoneNumber = request.PhoneNumber;

		await _context.SaveChangesAsync();

		return Ok(new
		{
			user.Id,
			user.FullName,
			user.Email,
			user.PhoneNumber
		});
	}
}