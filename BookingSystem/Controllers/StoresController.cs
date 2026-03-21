using BookingSystem.Data;
using BookingSystem.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly BookingDbContext _context;

    public StoresController(BookingDbContext context)
    {
        _context = context;
    }

    // ===========================
    // GET STORES (MAIN API)
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetStores(
    string sort = "recommended",
    double? lat = null,
    double? lng = null,
    double radius = 20,

    Guid? speciesId = null,
    string? categoryIds = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,

    decimal? minRating = null,
    string? search = null
)
    {
        IQueryable<Store> query = _context.Stores
            .Where(s => s.Latitude != null && s.Longitude != null);

        // === PARSE CATEGORY IDS ===
        List<Guid>? parsedCategoryIds = null;
        if (!string.IsNullOrWhiteSpace(categoryIds))
        {
            parsedCategoryIds = categoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id.Trim(), out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();
        }

        // FILTERS
        if (speciesId.HasValue)
        {
            query = query.Where(s => s.StoreSpecies.Any(ss => ss.SpeciesId == speciesId.Value));
        }

        if (parsedCategoryIds != null && parsedCategoryIds.Any())
        {
            query = query.Where(s => s.StoreCategories.Any(sc => parsedCategoryIds.Contains(sc.CategoryId)));
        }

        if (minRating.HasValue)
        {
            query = query.Where(s => s.AverageRating >= minRating.Value);
        }

        // Price filter
        if (minPrice.HasValue || maxPrice.HasValue)
        {
            query = query.Where(s => s.Services.Any(se =>
                (!minPrice.HasValue || se.Price >= minPrice.Value) &&
                (!maxPrice.HasValue || se.Price <= maxPrice.Value)
            ));
        }

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(s => EF.Functions.Like(s.Name, $"%{searchTerm}%"));
        }

        // PROJECTION
        var projection = query.Select(s => new
        {
            s.Id,
            s.Name,
            s.Address,
            s.Latitude,
            s.Longitude,
            s.AverageRating,
            s.ReviewCount,
            s.TotalCompletedBookings,

            MinPrice = s.Services.Any() ? s.Services.Min(se => se.Price) : 0m,

            Thumbnail = s.Images
                .Where(i => i.IsThumbnail)
                .OrderBy(i => i.Order)
                .Select(i => i.ImageUrl)
                .FirstOrDefault(),

            CategoryMatchCount = parsedCategoryIds != null && parsedCategoryIds.Any()
                ? s.StoreCategories.Count(sc => parsedCategoryIds.Contains(sc.CategoryId))
                : 0
        });

        var stores = await projection.ToListAsync();

        // Tính distance + score...
        var result = stores.Select(s =>
        {
            double distance = double.MaxValue;
            if (lat.HasValue && lng.HasValue && s.Latitude.HasValue && s.Longitude.HasValue)
            {
                distance = GetDistanceKm(lat.Value, lng.Value, (double)s.Latitude.Value, (double)s.Longitude.Value);
            }

            var score =
                (s.AverageRating * 0.5m) +
                ((decimal)Math.Log(s.ReviewCount + 1) * 0.25m) +
                ((decimal)Math.Log(s.TotalCompletedBookings + 1) * 0.25m);

            if (distance != double.MaxValue)
                score += (decimal)(1 / (distance + 1)) * 0.2m;

            return new
            {
                s.Id,
                s.Name,
                s.Address,
                s.Latitude,
                s.Longitude,
                s.AverageRating,
                s.ReviewCount,
                s.TotalCompletedBookings,
                s.MinPrice,
                s.Thumbnail,
                Distance = distance,
                Score = score,
                s.CategoryMatchCount
            };
        }).AsEnumerable();

        if (lat.HasValue && lng.HasValue)
            result = result.Where(r => r.Distance <= radius);

        // SORT
        result = sort switch
        {
            "distance" => result.OrderBy(r => r.Distance),
            "price" => result.OrderBy(r => r.MinPrice),
            "rating" => result.OrderByDescending(r => r.AverageRating),
            "booking" => result.OrderByDescending(r => r.TotalCompletedBookings),
            "category" => result.OrderByDescending(r => r.CategoryMatchCount),
            _ => result.OrderByDescending(r => r.Score)
        };

        return Ok(result.ToList());
    }

    // ===========================
    // GET STORE DETAIL
    // ===========================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var store = await _context.Stores
            .Include(s => s.Images)
            .Include(s => s.Reviews)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (store == null)
            return NotFound("Store not found");

        return Ok(new
        {
            store.Id,
            store.Name,
            store.Address,
            store.AverageRating,
            store.ReviewCount,
            store.TotalCompletedBookings,
            Images = store.Images
                .OrderBy(i => i.Order)
                .Select(i => new
                {
                    i.ImageUrl,
                    i.IsThumbnail
                })
                .ToList()
        });
    }

    // ===========================
    // FILTER DATA (REAL DB)
    // ===========================
    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters()
    {
        var species = await _context.Species
            .Select(s => new
            {
                s.Id,
                s.Name
            })
            .ToListAsync();

        var categories = await _context.StoreCategories
            .Select(c => new
            {
                c.Id,
                c.Name
            })
            .ToListAsync();

        return Ok(new
        {
            species,
            categories
        });
    }

    // ===========================
    // DISTANCE CALC
    // ===========================
    private double GetDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371.0;

        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(lat1 * Math.PI / 180) *
            Math.Cos(lat2 * Math.PI / 180) *
            Math.Sin(dLon / 2) *
            Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}