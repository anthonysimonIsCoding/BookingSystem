using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using BookingSystem.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookingSystem.Controllers;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingDbContext _context;

    public BookingsController(BookingDbContext context)
    {
        _context = context;
    }

    // POST api/bookings
    
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        // 🔥 1️⃣ Check slot tồn tại
        var slot = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.Id == request.TimeSlotId);

        if (slot == null)
            return NotFound("Time slot not found");

        // 🔥 2️⃣ Đếm số booking hiện tại
        var bookingCount = await _context.Bookings
            .CountAsync(b =>
                b.StoreId == request.StoreId &&
                b.TimeSlotId == request.TimeSlotId &&
                b.BookingDate == request.BookingDate);

        // 🔥 3️⃣ Check capacity
        if (bookingCount >= slot.Capacity)
            return BadRequest("Slot is full");

        // 🔥 4️⃣ Tạo booking
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userId),
            StoreId = request.StoreId,
            TimeSlotId = request.TimeSlotId,
            BookingDate = request.BookingDate,
            Status = BookingStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            booking.Id,
            booking.StoreId,
            booking.TimeSlotId,
            booking.BookingDate,
            booking.Status
        });
    }
    // GET api/bookings?date=2026-02-23
    [HttpGet]
    public async Task<IActionResult> GetByDate(DateOnly? date)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.TimeSlot)
            .AsQueryable();

        if (date.HasValue)
            query = query.Where(b => b.BookingDate == date.Value);

        var result = await query.ToListAsync();
        return Ok(result);
    }
}

public class CreateBookingRequest
{
    public Guid UserId { get; set; }
    public Guid StoreId { get; set; }
    public Guid TimeSlotId { get; set; }
    public DateOnly BookingDate { get; set; }
}