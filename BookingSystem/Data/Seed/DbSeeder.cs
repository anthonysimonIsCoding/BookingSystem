using BCrypt.Net;
using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingSystem.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(BookingDbContext context)
    {
        if (context.Users.Any()) return;

        var now = DateTime.UtcNow;
        var tomorrow = DateOnly.FromDateTime(now.AddDays(1));
        var nextWeek = DateOnly.FromDateTime(now.AddDays(7));

        // ================= BASIC DATA =================
        await SeedSpeciesAndBreeds(context, now);
        var (admin, customers, serviceProviders) = await SeedUsers(context, now);

        // ================= STORE CATEGORIES =================
        var categories = await SeedStoreCategories(context, now);

        // ================= STORES + ALL RELATED =================
        var stores = await SeedStoresAndRelated(context, now, categories, serviceProviders);

        // ================= TIME SLOT OVERRIDES (mới) =================
        await SeedTimeSlotOverrides(context, now, stores);

        // ================= PETS =================
        var pets = await SeedPets(context, now, customers);

        // ================= VOUCHERS =================
        var platformVoucher = await SeedPlatformVouchers(context, now);
        var storeVouchers = await SeedStoreVouchers(context, now, stores, serviceProviders);

        // ================= BOOKINGS + SERVICE ITEMS + USED VOUCHERS =================
        await SeedBookingsAndRelated(context, now, tomorrow, nextWeek, customers, stores, pets, platformVoucher, storeVouchers);

        // ================= REVIEWS =================
        await SeedReviews(context, now, customers, stores);

        // ================= FINAL SAVE =================
        await context.SaveChangesAsync();

        Console.WriteLine("🔥 SEED DONE - FULL DATASET (5 stores, 5 customers, 1 admin, 5 service providers, đầy đủ mọi entity + relation)");
    }

    // ================= SPECIES & BREEDS =================
    private static async Task SeedSpeciesAndBreeds(BookingDbContext context, DateTime now)
    {
        var dog = new Species { Id = Guid.NewGuid(), Name = "Dog", CreatedAt = now };
        var cat = new Species { Id = Guid.NewGuid(), Name = "Cat", CreatedAt = now };
        var rabbit = new Species { Id = Guid.NewGuid(), Name = "Rabbit", CreatedAt = now };

        context.Species.AddRange(dog, cat, rabbit);
        context.Breeds.AddRange(
            new Breed { Id = Guid.NewGuid(), SpeciesId = dog.Id, Name = "Poodle", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = dog.Id, Name = "Golden Retriever", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = dog.Id, Name = "Husky", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = cat.Id, Name = "Persian", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = cat.Id, Name = "Maine Coon", CreatedAt = now },
            new Breed { Id = Guid.NewGuid(), SpeciesId = rabbit.Id, Name = "Holland Lop", CreatedAt = now }
        );

        await context.SaveChangesAsync();
    }

    // ================= USERS =================
    private static async Task<(User admin, List<User> customers, List<User> serviceProviders)> SeedUsers(BookingDbContext context, DateTime now)
    {
        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Admin Master",
            Email = "admin@petbooking.com",
            PhoneNumber = "0987654321",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = UserRole.Admin,
            CreatedAt = now
        };

        var customers = new List<User>();
        for (int i = 1; i <= 5; i++)
        {
            customers.Add(new User
            {
                Id = Guid.NewGuid(),
                FullName = $"Khách hàng {i}",
                Email = $"customer{i}@test.com",
                PhoneNumber = $"09{i}1234567",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.Customer,
                CreatedAt = now
            });
        }

        var serviceProviders = new List<User>();
        for (int i = 1; i <= 5; i++)
        {
            serviceProviders.Add(new User
            {
                Id = Guid.NewGuid(),
                FullName = $"Chủ cửa hàng {i}",
                Email = $"owner{i}@petbooking.com",
                PhoneNumber = $"09{i}9876543",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.ServiceProvider,
                CreatedAt = now
            });
        }

        context.Users.Add(admin);
        context.Users.AddRange(customers);
        context.Users.AddRange(serviceProviders);
        await context.SaveChangesAsync();

        return (admin, customers, serviceProviders);
    }

    // ================= STORE CATEGORIES =================
    private static async Task<List<StoreCategory>> SeedStoreCategories(BookingDbContext context, DateTime now)
    {
        var categories = new List<StoreCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Spa & Grooming", Description = "Tắm rửa, cắt tỉa lông", CreatedAt = now, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Khách sạn thú cưng", Description = "Ở lại qua đêm", CreatedAt = now, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Daycare", Description = "Giữ ban ngày", CreatedAt = now, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Dog Walking", Description = "Dắt chó đi dạo", CreatedAt = now, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Boarding", Description = "Chăm sóc dài ngày", CreatedAt = now, IsActive = true }
        };

        context.StoreCategories.AddRange(categories);
        await context.SaveChangesAsync();
        return categories;
    }

    // ================= STORES + RELATED DATA (SỬA LỖI OwnerId) =================
    private static async Task<List<Store>> SeedStoresAndRelated(
        BookingDbContext context,
        DateTime now,
        List<StoreCategory> categories,
        List<User> serviceProviders)
    {
        var species = await context.Species.ToListAsync();
        var dog = species.First(s => s.Name == "Dog");
        var cat = species.First(s => s.Name == "Cat");

        var storeData = new[]
        {
            new { Name = "Pet Spa Deluxe", Address = "123 Nguyễn Huệ, Quận 1, TP.HCM", Lat = 10.7769m, Lng = 106.7009m, Owner = serviceProviders[0], Categories = new[] { categories[0], categories[2] } },
            new { Name = "Cat Paradise Hotel", Address = "45 Trần Phú, Quận Ba Đình, Hà Nội", Lat = 21.0285m, Lng = 105.8048m, Owner = serviceProviders[1], Categories = new[] { categories[1], categories[4] } },
            new { Name = "Dog Daycare Center", Address = "78 Bạch Đằng, Quận Hải Châu, Đà Nẵng", Lat = 16.0544m, Lng = 108.2022m, Owner = serviceProviders[2], Categories = new[] { categories[2], categories[3] } },
            new { Name = "Grooming Pro", Address = "12 Lý Thường Kiệt, Quận Ninh Kiều, Cần Thơ", Lat = 10.0333m, Lng = 105.7833m, Owner = serviceProviders[3], Categories = new[] { categories[0] } },
            new { Name = "Pet Boarding Paradise", Address = "99 Hùng Vương, TP. Nha Trang", Lat = 12.2388m, Lng = 109.1967m, Owner = serviceProviders[4], Categories = new[] { categories[1], categories[4] } }
        };

        var stores = new List<Store>();

        for (int i = 0; i < storeData.Length; i++)
        {
            var data = storeData[i];

            var store = new Store
            {
                Id = Guid.NewGuid(),
                Name = data.Name,
                Address = data.Address,
                Latitude = data.Lat,
                Longitude = data.Lng,
                CreatedAt = now,
                Status = StoreStatus.Approved,
                AverageRating = i % 2 == 0 ? 4.8m : 4.5m,
                ReviewCount = i % 2 == 0 ? 25 : 18,
                TotalCompletedBookings = i % 2 == 0 ? 120 : 85,
                OwnerId = data.Owner.Id   // ← SỬA LỖI QUAN TRỌNG
            };

            // StoreSpecies
            store.StoreSpecies = new List<StoreSpecies>
            {
                new() { StoreId = store.Id, SpeciesId = dog.Id },
                new() { StoreId = store.Id, SpeciesId = cat.Id }
            };

            // StoreCategoryMapping
            store.StoreCategories = data.Categories.Select(c => new StoreCategoryMapping
            {
                StoreId = store.Id,
                CategoryId = c.Id
            }).ToList();

            // Store Images
            store.Images = new List<StoreImage>
            {
                new() { Id = Guid.NewGuid(), StoreId = store.Id, ImageUrl = $"https://storage.example.com/stores/{i+1}/1.jpg", IsThumbnail = true, Order = 0, CreatedAt = now },
                new() { Id = Guid.NewGuid(), StoreId = store.Id, ImageUrl = $"https://storage.example.com/stores/{i+1}/2.jpg", IsThumbnail = false, Order = 1, CreatedAt = now },
                new() { Id = Guid.NewGuid(), StoreId = store.Id, ImageUrl = $"https://storage.example.com/stores/{i+1}/3.jpg", IsThumbnail = false, Order = 2, CreatedAt = now }
            };

            // TimeSlots
            store.TimeSlots = new List<TimeSlot>
            {
                new() { Id = Guid.NewGuid(), StoreId = store.Id, StartTime = new TimeSpan(8,0,0), EndTime = new TimeSpan(10,0,0), Capacity = 4, IsActive = true, CreatedAt = now },
                new() { Id = Guid.NewGuid(), StoreId = store.Id, StartTime = new TimeSpan(10,0,0), EndTime = new TimeSpan(12,0,0), Capacity = 4, IsActive = true, CreatedAt = now },
                new() { Id = Guid.NewGuid(), StoreId = store.Id, StartTime = new TimeSpan(13,0,0), EndTime = new TimeSpan(15,0,0), Capacity = 3, IsActive = true, CreatedAt = now },
                new() { Id = Guid.NewGuid(), StoreId = store.Id, StartTime = new TimeSpan(15,0,0), EndTime = new TimeSpan(17,0,0), Capacity = 3, IsActive = true, CreatedAt = now }
            };

            // Services + OptionGroups + Options (giữ nguyên logic cũ)
            var grooming = new Service
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Name = "Grooming Full Package",
                Description = "Tắm + cắt tỉa + chải lông",
                Price = 350000,
                DurationMinutes = 90,
                Type = ServiceType.Multiple,
                IsActive = true
            };

            var boarding = new Service
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Name = "Boarding 1 ngày",
                Description = "Chăm sóc qua đêm",
                Price = 450000,
                DurationMinutes = 1440,
                Type = ServiceType.Single,
                IsActive = true
            };

            // Option groups for Grooming
            var sizeGroup = new ServiceOptionGroup
            {
                Id = Guid.NewGuid(),
                ServiceId = grooming.Id,
                Name = "Kích thước thú cưng",
                Type = OptionGroupType.SingleChoice,
                IsRequired = true
            };
            sizeGroup.Options = new List<ServiceOption>
            {
                new() { Id = Guid.NewGuid(), OptionGroupId = sizeGroup.Id, Name = "Nhỏ (<10kg)", Price = 0, DurationMinutes = 0, IsActive = true },
                new() { Id = Guid.NewGuid(), OptionGroupId = sizeGroup.Id, Name = "Trung (10-25kg)", Price = 50000, DurationMinutes = 15, IsActive = true },
                new() { Id = Guid.NewGuid(), OptionGroupId = sizeGroup.Id, Name = "Lớn (>25kg)", Price = 100000, DurationMinutes = 30, IsActive = true }
            };

            var addonGroup = new ServiceOptionGroup
            {
                Id = Guid.NewGuid(),
                ServiceId = grooming.Id,
                Name = "Dịch vụ thêm",
                Type = OptionGroupType.MultiChoice,
                IsRequired = false
            };
            addonGroup.Options = new List<ServiceOption>
            {
                new() { Id = Guid.NewGuid(), OptionGroupId = addonGroup.Id, Name = "Cắt móng", Price = 50000, DurationMinutes = 10, IsActive = true },
                new() { Id = Guid.NewGuid(), OptionGroupId = addonGroup.Id, Name = "Tẩy răng", Price = 150000, DurationMinutes = 20, IsActive = true }
            };

            grooming.OptionGroups = new List<ServiceOptionGroup> { sizeGroup, addonGroup };

            // Option group for Boarding
            var roomGroup = new ServiceOptionGroup
            {
                Id = Guid.NewGuid(),
                ServiceId = boarding.Id,
                Name = "Loại phòng",
                Type = OptionGroupType.SingleChoice,
                IsRequired = true
            };
            roomGroup.Options = new List<ServiceOption>
            {
                new() { Id = Guid.NewGuid(), OptionGroupId = roomGroup.Id, Name = "Phòng tiêu chuẩn", Price = 0, DurationMinutes = 0, IsActive = true },
                new() { Id = Guid.NewGuid(), OptionGroupId = roomGroup.Id, Name = "Phòng VIP (có camera)", Price = 150000, DurationMinutes = 0, IsActive = true }
            };
            boarding.OptionGroups = new List<ServiceOptionGroup> { roomGroup };

            store.Services = new List<Service> { grooming, boarding };

            context.Stores.Add(store);
            stores.Add(store);
        }

        await context.SaveChangesAsync();
        return stores;
    }

    // ================= TIME SLOT OVERRIDES (MỚI - entity bị thiếu) =================
    private static async Task SeedTimeSlotOverrides(BookingDbContext context, DateTime now, List<Store> stores)
    {
        var overrides = new List<TimeSlotOverride>();

        foreach (var store in stores)
        {
            // 1. Full day closure
            overrides.Add(new TimeSlotOverride
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Date = DateOnly.FromDateTime(now.AddDays(2)),
                IsFullDayClosure = true,
                Reason = "Nghỉ bảo dưỡng cửa hàng",
                CreatedAt = now,
                CreatedByUserId = store.OwnerId!.Value
            });

            // 2. Capacity override cho 1 timeslot
            var timeSlot = await context.TimeSlots
                .Where(ts => ts.StoreId == store.Id)
                .FirstAsync();

            overrides.Add(new TimeSlotOverride
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                TimeSlotId = timeSlot.Id,
                Date = DateOnly.FromDateTime(now.AddDays(1)),
                Capacity = 2,
                Reason = "Giảm slot do nhân viên nghỉ",
                CreatedAt = now,
                CreatedByUserId = store.OwnerId!.Value
            });
        }

        context.TimeSlotOverrides.AddRange(overrides);
        await context.SaveChangesAsync();
    }

    // ================= PETS =================
    private static async Task<List<Pet>> SeedPets(BookingDbContext context, DateTime now, List<User> customers)
    {
        var species = await context.Species.ToListAsync();
        var breeds = await context.Breeds.ToListAsync();

        var pets = new List<Pet>();

        foreach (var customer in customers)
        {
            pets.Add(new Pet
            {
                Id = Guid.NewGuid(),
                UserId = customer.Id,
                Name = $"Cún {customer.FullName.Split(' ').Last()}",
                SpeciesId = species.First(s => s.Name == "Dog").Id,
                BreedId = breeds.First(b => b.Name == "Poodle").Id,
                Gender = "Male",
                DateOfBirth = DateOnly.FromDateTime(now.AddYears(-2)),
                Color = "White",
                Weight = 8.5,
                Notes = "Rất ngoan, thích tắm",
                ProfileImageUrl = "https://storage.example.com/pets/dog1.jpg",
                CreatedAt = now,
                IsActive = true
            });

            pets.Add(new Pet
            {
                Id = Guid.NewGuid(),
                UserId = customer.Id,
                Name = $"Mèo {customer.FullName.Split(' ').Last()}",
                SpeciesId = species.First(s => s.Name == "Cat").Id,
                BreedId = breeds.First(b => b.Name == "Persian").Id,
                Gender = "Female",
                DateOfBirth = DateOnly.FromDateTime(now.AddYears(-1)),
                Color = "Gray",
                Weight = 4.2,
                Notes = "Hay kêu meo meo",
                ProfileImageUrl = "https://storage.example.com/pets/cat1.jpg",
                CreatedAt = now,
                IsActive = true
            });
        }

        context.Pets.AddRange(pets);
        await context.SaveChangesAsync();
        return pets;
    }

    // ================= PLATFORM VOUCHERS =================
    private static async Task<PlatformVoucher> SeedPlatformVouchers(BookingDbContext context, DateTime now)
    {
        var voucher = new PlatformVoucher
        {
            Id = Guid.NewGuid(),
            Code = "WELCOME2025",
            Name = "Chào mừng năm mới 2025",
            Description = "Giảm 20% cho đơn đầu tiên",
            DiscountType = VoucherDiscountType.Percent,
            DiscountValue = 20,
            MinOrderValue = 200000,
            MaxDiscountAmount = 150000,
            UsageLimitPerUser = 1,
            TotalUsageLimit = 500,
            StartDate = now.AddDays(-10),
            EndDate = now.AddMonths(3),
            IsActive = true,
            CreatedAt = now,
            CreatedByAdminId = context.Users.First(u => u.Role == UserRole.Admin).Id
        };

        context.PlatformVouchers.Add(voucher);
        await context.SaveChangesAsync();
        return voucher;
    }

    // ================= STORE VOUCHERS =================
    private static async Task<List<StoreVoucher>> SeedStoreVouchers(
        BookingDbContext context,
        DateTime now,
        List<Store> stores,
        List<User> serviceProviders)
    {
        var vouchers = new List<StoreVoucher>();

        for (int i = 0; i < stores.Count; i++)
        {
            var store = stores[i];
            var owner = serviceProviders[i];

            vouchers.Add(new StoreVoucher
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Code = $"STORE{i + 1}10",
                Name = "Giảm 10% dịch vụ",
                Description = "Áp dụng cho mọi dịch vụ",
                DiscountType = VoucherDiscountType.Percent,
                DiscountValue = 10,
                MinOrderValue = 100000,
                MaxDiscountAmount = 100000,
                UsageLimitPerUser = 2,
                TotalUsageLimit = 100,
                StartDate = now.AddDays(-5),
                EndDate = null,
                IsActive = true,
                CreatedAt = now,
                CreatedByStoreOwnerId = owner.Id
            });

            if (i % 2 == 0)
            {
                vouchers.Add(new StoreVoucher
                {
                    Id = Guid.NewGuid(),
                    StoreId = store.Id,
                    Code = $"STORE{i + 1}50K",
                    Name = "Giảm 50k",
                    Description = "Giảm cố định 50.000đ",
                    DiscountType = VoucherDiscountType.Fixed,
                    DiscountValue = 50000,
                    MinOrderValue = 300000,
                    MaxDiscountAmount = null,
                    UsageLimitPerUser = null,
                    TotalUsageLimit = null,
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddMonths(6),
                    IsActive = true,
                    CreatedAt = now,
                    CreatedByStoreOwnerId = owner.Id
                });
            }
        }

        context.StoreVouchers.AddRange(vouchers);
        await context.SaveChangesAsync();
        return vouchers;
    }

    // ================= BOOKINGS + SERVICE ITEMS + USED VOUCHERS =================
    private static async Task SeedBookingsAndRelated(
        BookingDbContext context,
        DateTime now,
        DateOnly tomorrow,
        DateOnly nextWeek,
        List<User> customers,
        List<Store> stores,
        List<Pet> pets,
        PlatformVoucher platformVoucher,
        List<StoreVoucher> storeVouchers)
    {
        var timeSlots = await context.TimeSlots.ToListAsync();
        var services = await context.Services
            .Include(s => s.OptionGroups)
            .ThenInclude(g => g.Options)
            .ToListAsync();

        var bookings = new List<Booking>();
        var serviceItems = new List<BookingServiceItem>();
        var usedVouchers = new List<UsedVoucher>();

        for (int i = 0; i < 12; i++)
        {
            var customer = customers[i % 5];
            var store = stores[i % 5];
            var pet = pets.First(p => p.UserId == customer.Id);
            var bookingTimeSlot = timeSlots.First(ts => ts.StoreId == store.Id);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = customer.Id,
                StoreId = store.Id,
                TimeSlotId = bookingTimeSlot.Id,
                PetId = pet.Id,
                BookingDate = i % 3 == 0 ? tomorrow : nextWeek,
                Status = i % 4 == 0 ? BookingStatus.Completed : BookingStatus.Pending,
                Notes = i % 2 == 0 ? "Pet hơi nhút nhát" : null,
                CreatedAt = now.AddDays(-i),
                TotalPrice = 450000 + (i * 50000m)
            };

            var service = services.First(s => s.StoreId == store.Id && s.Name.Contains("Grooming"));
            var sizeOption = service.OptionGroups.First(g => g.Name.Contains("Kích thước")).Options.First();
            var addonOption = service.OptionGroups.First(g => g.Name.Contains("Dịch vụ thêm")).Options.First();

            serviceItems.Add(new BookingServiceItem
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                ServiceOptionId = sizeOption.Id,
                Price = service.Price + sizeOption.Price,
                DurationMinutes = service.DurationMinutes + sizeOption.DurationMinutes
            });

            if (i % 2 == 0)
            {
                serviceItems.Add(new BookingServiceItem
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    ServiceOptionId = addonOption.Id,
                    Price = addonOption.Price,
                    DurationMinutes = addonOption.DurationMinutes
                });
            }

            // Voucher
            if (i % 3 == 0)
            {
                booking.PlatformVoucherId = platformVoucher.Id;
                booking.PlatformVoucherDiscount = 80000;
                usedVouchers.Add(new UsedVoucher
                {
                    Id = Guid.NewGuid(),
                    PlatformVoucherId = platformVoucher.Id,
                    UserId = customer.Id,
                    BookingId = booking.Id,
                    DiscountApplied = 80000,
                    UsedAt = now.AddDays(-i)
                });
            }
            else if (i % 4 == 1)
            {
                var storeVoucher = storeVouchers.First(v => v.StoreId == store.Id);
                booking.StoreVoucherId = storeVoucher.Id;
                booking.StoreVoucherDiscount = 50000;
                usedVouchers.Add(new UsedVoucher
                {
                    Id = Guid.NewGuid(),
                    StoreVoucherId = storeVoucher.Id,
                    UserId = customer.Id,
                    BookingId = booking.Id,
                    DiscountApplied = 50000,
                    UsedAt = now.AddDays(-i)
                });
            }

            bookings.Add(booking);
        }

        context.Bookings.AddRange(bookings);
        context.BookingServiceItems.AddRange(serviceItems);
        context.UsedVouchers.AddRange(usedVouchers);

        await context.SaveChangesAsync();
    }

    // ================= REVIEWS =================
    private static async Task SeedReviews(BookingDbContext context, DateTime now, List<User> customers, List<Store> stores)
    {
        var reviews = new List<Review>();
        for (int i = 0; i < 15; i++)
        {
            reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                StoreId = stores[i % 5].Id,
                UserId = customers[i % 5].Id,
                Rating = i % 5 == 0 ? 5.0m : 4.5m,
                Comment = i % 3 == 0 ? "Dịch vụ tuyệt vời, pet rất vui!" : "Cửa hàng sạch sẽ, nhân viên thân thiện.",
                CreatedAt = now.AddDays(-i * 2)
            });
        }

        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }
}