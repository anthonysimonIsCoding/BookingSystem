using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using BookingSystem.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookingSystem.DTOs;  // hoặc namespace của DTO

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


        var slot = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.Id == request.TimeSlotId);

        if (slot == null)
            return NotFound("Time slot not found");


        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == request.PetId && p.UserId == Guid.Parse(userId));

        if (pet == null)
            return BadRequest("Pet không tồn tại");


        var bookingCount = await _context.Bookings
            .CountAsync(b =>
                b.TimeSlotId == request.TimeSlotId &&
                b.BookingDate == request.BookingDate);


        if (bookingCount >= slot.Capacity)
            return BadRequest("Slot is full");


        var services = await _context.Services
            .Where(s => request.ServiceIds.Contains(s.Id))
            .ToListAsync();


        decimal totalPrice = services.Sum(s => s.Price);


        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userId),
            StoreId = request.StoreId,
            TimeSlotId = request.TimeSlotId,
            PetId = request.PetId,
            BookingDate = request.BookingDate,
            Status = BookingStatus.Active,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = totalPrice
        };


        _context.Bookings.Add(booking);


        foreach (var service in services)
        {
            _context.BookingServices.Add(new BookingService
            {
                BookingId = booking.Id,
                ServiceId = service.Id,
                Price = service.Price
            });
        }


        await _context.SaveChangesAsync();


        return Ok(new
        {
            booking.Id,
            booking.TotalPrice
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

    // GET api/bookings/pet/{petId}
    [HttpGet("pet/{petId}")]
    public async Task<IActionResult> GetPetHistory(Guid petId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        // check pet có thuộc user không
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == petId && p.UserId == Guid.Parse(userId));

        if (pet == null)
            return NotFound("Pet not found");

        var history = await _context.Bookings
            .Where(b => b.PetId == petId)
            .Include(b => b.Store)
            .Include(b => b.TimeSlot)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new
            {
                b.Id,
                StoreName = b.Store.Name,
                ServiceNames = b.BookingServices
                    .Select(bs => bs.Service.Name)
                    .ToList(),
                BookingDate = b.BookingDate,
                StartTime = b.TimeSlot.StartTime.ToString(@"hh\:mm")
            })
            .ToListAsync();

        return Ok(history);
    }
}
