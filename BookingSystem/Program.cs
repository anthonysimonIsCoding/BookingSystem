using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using BookingSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = "THIS_IS_A_SUPER_SECRET_KEY_1234567890";

// ================= DB =================

builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ================= Services =================

builder.Services.AddScoped<AuthService>();


// ================= JWT =================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();


// ================= Swagger =================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new string[] {}
        }
    });
});


// ================= Controllers =================

builder.Services.AddControllers();


// ================= CORS =================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();


// ================= SEED DATA =================

// ================= SEED DATA =================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();

    context.Database.EnsureCreated();

    if (!context.Users.Any())
    {
        var now = DateTime.UtcNow;
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var dayAfter = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));

        // ───────────────────────────────────────────
        // SPECIES + BREEDS
        // ───────────────────────────────────────────
        var dog = new Species { Id = Guid.NewGuid(), Name = "Dog", CreatedAt = now };
        var cat = new Species { Id = Guid.NewGuid(), Name = "Cat", CreatedAt = now };
        var rabbit = new Species { Id = Guid.NewGuid(), Name = "Rabbit", CreatedAt = now };

        context.Species.AddRange(dog, cat, rabbit);

        var breeds = new[]
        {
            new Breed { Id = Guid.NewGuid(), SpeciesId = dog.Id, Name = "Poodle", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = dog.Id, Name = "Husky", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = dog.Id, Name = "Golden Retriever", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = cat.Id, Name = "British Shorthair", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = cat.Id, Name = "Persian", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = rabbit.Id, Name = "Holland Lop", CreatedAt = now },
        };
        context.Breeds.AddRange(breeds);

        context.SaveChanges();

        // ───────────────────────────────────────────
        // STORE CATEGORIES
        // ───────────────────────────────────────────
        var spa = new StoreCategory { Id = Guid.NewGuid(), Name = "Spa & Grooming", CreatedAt = now };
        var hotel = new StoreCategory { Id = Guid.NewGuid(), Name = "Pet Hotel", CreatedAt = now };
        var daycare = new StoreCategory { Id = Guid.NewGuid(), Name = "Daycare", CreatedAt = now };
        var walking = new StoreCategory { Id = Guid.NewGuid(), Name = "Dog Walking", CreatedAt = now };

        context.StoreCategories.AddRange(spa, hotel, daycare, walking);
        context.SaveChanges();

        // ───────────────────────────────────────────
        // USERS
        // ───────────────────────────────────────────
        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Admin System",
            Email = "admin@bookingsystem.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123!"),
            Role = UserRole.Admin,
            CreatedAt = now
        };

        var khanh = new User
        {
            Id = Guid.NewGuid(),
            FullName = "the Khánh",
            Email = "khanh@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Customer,
            CreatedAt = now
        };

        var lan = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Lan Nguyễn",
            Email = "lan@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("matkhau123"),
            Role = UserRole.Customer,
            CreatedAt = now
        };

        context.Users.AddRange(admin, khanh, lan);
        context.SaveChanges();

        // ───────────────────────────────────────────
        // STORES
        // ───────────────────────────────────────────
        var stores = new[]
        {
            new Store { Id = Guid.NewGuid(), Name = "Pet Luxury Spa",      Address = "45 Pasteur, Quận 1",         AverageRating = 4.7m, ReviewCount = 89,  TotalCompletedBookings = 0, CreatedAt = now },
            new Store { Id = Guid.NewGuid(), Name = "Happy Paws Hotel",    Address = "112 Lê Văn Sỹ, Phú Nhuận",   AverageRating = 4.9m, ReviewCount = 214, TotalCompletedBookings = 0, CreatedAt = now },
            new Store { Id = Guid.NewGuid(), Name = "Pet House Daycare",   Address = "78 Nguyễn Thị Minh Khai, Q3", AverageRating = 4.4m, ReviewCount = 67,  TotalCompletedBookings = 0, CreatedAt = now },
        };

        context.Stores.AddRange(stores);
        context.SaveChanges();

        // Store ↔ Category
        context.StoreCategoryMappings.AddRange(
            new StoreCategoryMapping { StoreId = stores[0].Id, CategoryId = spa.Id },
            new StoreCategoryMapping { StoreId = stores[0].Id, CategoryId = daycare.Id },
            new StoreCategoryMapping { StoreId = stores[1].Id, CategoryId = hotel.Id },
            new StoreCategoryMapping { StoreId = stores[1].Id, CategoryId = spa.Id },
            new StoreCategoryMapping { StoreId = stores[2].Id, CategoryId = daycare.Id },
            new StoreCategoryMapping { StoreId = stores[2].Id, CategoryId = walking.Id }
        );

        // Store ↔ Species
        context.StoreSpecies.AddRange(
            new StoreSpecies { StoreId = stores[0].Id, SpeciesId = dog.Id },
            new StoreSpecies { StoreId = stores[0].Id, SpeciesId = cat.Id },
            new StoreSpecies { StoreId = stores[1].Id, SpeciesId = dog.Id },
            new StoreSpecies { StoreId = stores[1].Id, SpeciesId = cat.Id },
            new StoreSpecies { StoreId = stores[1].Id, SpeciesId = rabbit.Id },
            new StoreSpecies { StoreId = stores[2].Id, SpeciesId = dog.Id }
        );

        context.SaveChanges();

        // ───────────────────────────────────────────
        // SERVICES (mỗi store có vài dịch vụ)
        // ───────────────────────────────────────────
        var services = new List<Service>();

        // Store 0 - Spa
        services.AddRange(new[]
        {
            new Service { Id = Guid.NewGuid(), StoreId = stores[0].Id, Name = "Full Grooming",       Price = 280_000, DurationMinutes = 120},
            new Service { Id = Guid.NewGuid(), StoreId = stores[0].Id, Name = "Bath + Brush",        Price = 150_000, DurationMinutes = 60},
            new Service { Id = Guid.NewGuid(), StoreId = stores[0].Id, Name = "Nail Trim & Ear Clean", Price = 90_000, DurationMinutes = 30},
        });

        // Store 1 - Hotel
        services.AddRange(new[]
        {
            new Service { Id = Guid.NewGuid(), StoreId = stores[1].Id, Name = "Overnight Stay (small)", Price = 350_000, DurationMinutes = 1440},
            new Service { Id = Guid.NewGuid(), StoreId = stores[1].Id, Name = "Overnight Stay (large)", Price = 480_000, DurationMinutes = 1440},
        });

        // Store 2 - Daycare
        services.AddRange(new[]
        {
            new Service { Id = Guid.NewGuid(), StoreId = stores[2].Id, Name = "Daycare Full Day",    Price = 180_000, DurationMinutes = 480},
            new Service { Id = Guid.NewGuid(), StoreId = stores[2].Id, Name = "Daycare Half Day",    Price = 110_000, DurationMinutes = 240},
        });

        context.Services.AddRange(services);
        context.SaveChanges();

        // ───────────────────────────────────────────
        // TIME SLOTS (8h-18h, mỗi slot 1 tiếng, capacity 5)
        // ───────────────────────────────────────────
        foreach (var store in stores)
        {
            var slots = new List<TimeSlot>();
            for (int h = 8; h < 18; h++)
            {
                slots.Add(new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    StoreId = store.Id,
                    StartTime = TimeSpan.FromHours(h),
                    EndTime = TimeSpan.FromHours(h + 1),
                    Capacity = 5,
                    IsActive = true,
                    CreatedAt = now
                });
            }
            context.TimeSlots.AddRange(slots);
        }
        context.SaveChanges();

        // ───────────────────────────────────────────
        // PETS
        // ───────────────────────────────────────────
        var pets = new[]
        {
            new Pet { Id = Guid.NewGuid(), UserId = khanh.Id, Name = "Bông",   SpeciesId = dog.Id, BreedId = breeds[0].Id, Gender = "Male",   Weight = 5.8,  CreatedAt = now },
            new Pet { Id = Guid.NewGuid(), UserId = khanh.Id, Name = "Mun",    SpeciesId = cat.Id, BreedId = breeds[4].Id, Gender = "Female", Weight = 3.9,  CreatedAt = now },
            new Pet { Id = Guid.NewGuid(), UserId = lan.Id,   Name = "Lucky",  SpeciesId = dog.Id, BreedId = breeds[2].Id, Gender = "Male",   Weight = 28.5, CreatedAt = now },
        };
        context.Pets.AddRange(pets);
        context.SaveChanges();

        // ───────────────────────────────────────────
        // BOOKINGS + BOOKING SERVICES
        // ───────────────────────────────────────────
        var bookings = new List<Booking>();

        // Booking hoàn thành (để test TotalCompletedBookings)
        bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            UserId = khanh.Id,
            StoreId = stores[0].Id,
            PetId = pets[0].Id,
            TimeSlotId = context.TimeSlots.First(t => t.StoreId == stores[0].Id).Id,
            BookingDate = DateOnly.FromDateTime(now.AddDays(-5)),
            Status = BookingStatus.Completed,
            TotalPrice = 280_000,
            CreatedAt = now.AddDays(-6),
        });

        // Booking sắp tới
        bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            UserId = khanh.Id,
            StoreId = stores[1].Id,
            PetId = pets[1].Id,
            TimeSlotId = context.TimeSlots.First(t => t.StoreId == stores[1].Id).Id,
            BookingDate = tomorrow,
            Status = BookingStatus.Active,
            TotalPrice = 350_000,
            CreatedAt = now,
        });

        bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            UserId = lan.Id,
            StoreId = stores[2].Id,
            PetId = pets[2].Id,
            TimeSlotId = context.TimeSlots.First(t => t.StoreId == stores[2].Id).Id,
            BookingDate = dayAfter,
            Status = BookingStatus.Active,
            TotalPrice = 180_000,
            CreatedAt = now,
        });

        context.Bookings.AddRange(bookings);
        context.SaveChanges();

        // Liên kết dịch vụ với booking
        context.BookingServices.AddRange(
            new BookingService { BookingId = bookings[0].Id, ServiceId = services[0].Id, Price = services[0].Price },   // grooming
            new BookingService { BookingId = bookings[1].Id, ServiceId = services[3].Id, Price = services[3].Price },   // overnight small
            new BookingService { BookingId = bookings[2].Id, ServiceId = services[6].Id, Price = services[6].Price }    // daycare full day
        );

        // Cập nhật TotalCompletedBookings cho store đã hoàn thành
        stores[0].TotalCompletedBookings = 1;

        context.SaveChanges();

        // ───────────────────────────────────────────
        // REVIEWS (để test rating)
        // ───────────────────────────────────────────
        context.Reviews.AddRange(
            new Review { Id = Guid.NewGuid(), StoreId = stores[0].Id, UserId = khanh.Id, Rating = 5.0m, Comment = "Rất cẩn thận và sạch sẽ!", CreatedAt = now.AddDays(-3) },
            new Review { Id = Guid.NewGuid(), StoreId = stores[0].Id, UserId = lan.Id, Rating = 4.0m, Comment = "Tốt nhưng hơi đông", CreatedAt = now.AddDays(-10) },
            new Review { Id = Guid.NewGuid(), StoreId = stores[1].Id, UserId = khanh.Id, Rating = 5.0m, Comment = "Chuồng rộng rãi, bé chơi vui cả ngày", CreatedAt = now.AddDays(-1) }
        );

        context.SaveChanges();

        // ───────────────────────────────────────────
        // STORE IMAGES (thumbnail + 2-3 ảnh phụ mỗi store)
        // ───────────────────────────────────────────
        var storeImages = new List<StoreImage>();

        foreach (var store in stores)
        {
            // Thumbnail chính (Order = 0)
            storeImages.Add(new StoreImage
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                ImageUrl = $"https://example.com/images/stores/{store.Name.ToLower().Replace(" ", "-")}/thumbnail.jpg",
                IsThumbnail = true,
                Order = 0,
                CreatedAt = now
            });

            // Ảnh phụ
            storeImages.AddRange(new[]
            {
        new StoreImage
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            ImageUrl = $"https://example.com/images/stores/{store.Name.ToLower().Replace(" ", "-")}/interior-1.jpg",
            IsThumbnail = false,
            Order = 1,
            CreatedAt = now
        },
        new StoreImage
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            ImageUrl = $"https://example.com/images/stores/{store.Name.ToLower().Replace(" ", "-")}/pets-playing.jpg",
            IsThumbnail = false,
            Order = 2,
            CreatedAt = now
        },
        new StoreImage
        {
            Id = Guid.NewGuid(),
            StoreId = store.Id,
            ImageUrl = $"https://example.com/images/stores/{store.Name.ToLower().Replace(" ", "-")}/grooming-area.jpg",
            IsThumbnail = false,
            Order = 3,
            CreatedAt = now
        }
    });
        }

        context.StoreImages.AddRange(storeImages);
        context.SaveChanges();

        Console.WriteLine($"Seeded {storeImages.Count} store images.");

        // ───────────────────────────────────────────
        // TIME SLOT OVERRIDES
        // ───────────────────────────────────────────
        // Ví dụ 1: Full day closure (đóng cửa hoàn toàn ngày mai) cho store 0
        // Ví dụ 2: Override capacity + thời gian cho 1 slot cụ thể ở store 1
        // Ví dụ 3: Tắt 1 slot buổi chiều ở store 2

        var timeSlotOverrides = new List<TimeSlotOverride>();

        // 1. Đóng cửa toàn bộ ngày mai (tomorrow) - store 0
        timeSlotOverrides.Add(new TimeSlotOverride
        {
            Id = Guid.NewGuid(),
            StoreId = stores[0].Id,
            TimeSlotId = null,                    // null → áp dụng full day
            Date = tomorrow,
            IsFullDayClosure = true,
            Reason = "Nghỉ lễ Quốc khánh (dọn dẹp lớn)",
            CreatedAt = now,
            CreatedByUserId = admin.Id            // Admin tạo
        });

        // 2. Override slot 9h-10h ngày kia (dayAfter) - tăng capacity lên 8 cho store 1
        var morningSlotStore1 = context.TimeSlots
            .FirstOrDefault(t => t.StoreId == stores[1].Id && t.StartTime == TimeSpan.FromHours(9));

        if (morningSlotStore1 != null)
        {
            timeSlotOverrides.Add(new TimeSlotOverride
            {
                Id = Guid.NewGuid(),
                StoreId = stores[1].Id,
                TimeSlotId = morningSlotStore1.Id,
                Date = dayAfter,
                Capacity = 8,                         // tăng từ 5 lên 8
                StartTime = null,                     // giữ nguyên
                EndTime = null,
                IsActive = true,
                Reason = "Nhận thêm khách đoàn do có sự kiện",
                CreatedAt = now,
                CreatedByUserId = admin.Id
            });
        }

        // 3. Tắt slot 16h-17h ngày mai cho store 2 (ví dụ bảo trì máy sấy)
        var afternoonSlotStore2 = context.TimeSlots
            .FirstOrDefault(t => t.StoreId == stores[2].Id && t.StartTime == TimeSpan.FromHours(16));

        if (afternoonSlotStore2 != null)
        {
            timeSlotOverrides.Add(new TimeSlotOverride
            {
                Id = Guid.NewGuid(),
                StoreId = stores[2].Id,
                TimeSlotId = afternoonSlotStore2.Id,
                Date = tomorrow,
                IsActive = false,
                Reason = "Bảo trì thiết bị (máy sấy tạm nghỉ)",
                CreatedAt = now,
                CreatedByUserId = admin.Id
            });
        }

        context.TimeSlotOverrides.AddRange(timeSlotOverrides);
        context.SaveChanges();

        // ───────────────────────────────────────────
        // PLATFORM VOUCHERS (toàn sàn)
        // ───────────────────────────────────────────
        var platformVouchers = new[]
        {
    new PlatformVoucher
    {
        Id = Guid.NewGuid(),
        Code = "WELCOME30",
        Name = "Chào mừng thành viên mới",
        Description = "Giảm 30% cho đơn đầu tiên, tối đa 150.000đ",
        DiscountType = VoucherDiscountType.Percent,
        DiscountValue = 30m,
        MinOrderValue = 200_000m,
        MaxDiscountAmount = 150_000m,
        UsageLimitPerUser = 1,
        TotalUsageLimit = 500,
        StartDate = DateTime.UtcNow.AddDays(-10),
        EndDate = DateTime.UtcNow.AddDays(30),
        IsActive = true,
        CreatedAt = now,
        CreatedByAdminId = admin.Id
    },
    new PlatformVoucher
    {
        Id = Guid.NewGuid(),
        Code = "PETLOVE50K",
        Name = "Giảm thẳng 50k",
        Description = "Giảm 50.000đ cho mọi đơn từ 300k trở lên",
        DiscountType = VoucherDiscountType.Fixed,
        DiscountValue = 50_000m,
        MinOrderValue = 300_000m,
        StartDate = DateTime.UtcNow.AddDays(-5),
        EndDate = null,
        IsActive = true,
        CreatedAt = now,
        CreatedByAdminId = admin.Id
    }
};

        context.PlatformVouchers.AddRange(platformVouchers);
        context.SaveChanges();

        // ───────────────────────────────────────────
        // STORE VOUCHERS (riêng từng cửa hàng)
        // ───────────────────────────────────────────
        var storeVouchers = new[]
        {
    // Voucher cho Pet Luxury Spa (stores[0])
    new StoreVoucher
    {
        Id = Guid.NewGuid(),
        StoreId = stores[0].Id,
        Code = "SPA20",
        Name = "Giảm 20% grooming",
        Description = "Giảm 20% cho dịch vụ grooming, tối đa 100k",
        DiscountType = VoucherDiscountType.Percent,
        DiscountValue = 20m,
        MinOrderValue = 150_000m,
        MaxDiscountAmount = 100_000m,
        UsageLimitPerUser = 2,
        TotalUsageLimit = 200,
        StartDate = DateTime.UtcNow.AddDays(-7),
        EndDate = DateTime.UtcNow.AddDays(60),
        IsActive = true,
        ApplicableServiceId = services.First(s => s.Name.Contains("Grooming")).Id, // áp dụng cho grooming
        CreatedAt = now,
        CreatedByStoreOwnerId = admin.Id // tạm dùng admin, sau này có thể là owner store
    },

    // Voucher cho Happy Paws Hotel (stores[1])
    new StoreVoucher
    {
        Id = Guid.NewGuid(),
        StoreId = stores[1].Id,
        Code = "HOTEL100K",
        Name = "Giảm 100k lưu trú",
        Description = "Giảm thẳng 100k cho đơn lưu trú từ 2 đêm",
        DiscountType = VoucherDiscountType.Fixed,
        DiscountValue = 100_000m,
        MinOrderValue = 600_000m,
        StartDate = DateTime.UtcNow.AddDays(-3),
        EndDate = DateTime.UtcNow.AddDays(45),
        IsActive = true,
        CreatedAt = now,
        CreatedByStoreOwnerId = admin.Id
    }
};

        context.StoreVouchers.AddRange(storeVouchers);
        context.SaveChanges();

        // ───────────────────────────────────────────
        // USED VOUCHER (ví dụ: ai đã dùng voucher nào)
        // ───────────────────────────────────────────
        var usedVouchers = new[]
        {
    // Khánh dùng voucher toàn sàn cho booking cũ
    new UsedVoucher
    {
        Id = Guid.NewGuid(),
        PlatformVoucherId = platformVouchers[0].Id,
        UserId = khanh.Id,
        BookingId = bookings[0].Id, // booking hoàn thành trước đó
        DiscountApplied = 84_000m, // giả sử giảm 30% của 280k
        UsedAt = now.AddDays(-5)
    },

    // Lan dùng voucher của store cho booking sắp tới
    new UsedVoucher
    {
        Id = Guid.NewGuid(),
        StoreVoucherId = storeVouchers[1].Id,
        UserId = lan.Id,
        BookingId = bookings[2].Id,
        DiscountApplied = 100_000m,
        UsedAt = now
    }
};

        context.UsedVouchers.AddRange(usedVouchers);
        context.SaveChanges();

        Console.WriteLine("Seeded platform vouchers, store vouchers & used vouchers.");

        Console.WriteLine($"Seeded {timeSlotOverrides.Count} time slot overrides.");

        Console.WriteLine("=====================================");
        Console.WriteLine("       SEED DATA COMPLETED !         ");
        Console.WriteLine("=====================================");
    }
}


// ================= Middleware =================

app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseAuthentication();

app.UseAuthorization();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.Run();