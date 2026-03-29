using Microsoft.EntityFrameworkCore;
using BookingSystem.Entities;
using BookingSystem.Entities.Enums;

namespace BookingSystem.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreImage> StoreImages => Set<StoreImage>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<TimeSlotOverride> TimeSlotOverrides => Set<TimeSlotOverride>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceOptionGroup> ServiceOptionGroups => Set<ServiceOptionGroup>();
    public DbSet<ServiceOption> ServiceOptions => Set<ServiceOption>();
    public DbSet<BookingServiceItem> BookingServiceItems => Set<BookingServiceItem>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<Breed> Breeds => Set<Breed>();
    public DbSet<StoreSpecies> StoreSpecies => Set<StoreSpecies>();
    public DbSet<StoreCategory> StoreCategories => Set<StoreCategory>();
    public DbSet<StoreCategoryMapping> StoreCategoryMappings => Set<StoreCategoryMapping>();

    // Voucher
    public DbSet<PlatformVoucher> PlatformVouchers => Set<PlatformVoucher>();
    public DbSet<StoreVoucher> StoreVouchers => Set<StoreVoucher>();
    public DbSet<UsedVoucher> UsedVouchers => Set<UsedVoucher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === User ===
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired();                    // ← thêm
            entity.Property(e => e.CreatedAt).IsRequired();               // ← thêm
        });

        // === Store ===
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10,7)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10,7)");
            entity.Property(e => e.AverageRating).HasPrecision(3, 1);
            entity.Property(e => e.ReviewCount).HasDefaultValue(0);
            entity.Property(e => e.TotalCompletedBookings).HasDefaultValue(0);

            // ← THÊM MỚI
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasDefaultValue(StoreStatus.Pending);

            entity.HasOne(e => e.Owner)
                  .WithMany()                       // User chưa có collection OwnedStores
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === StoreImage ===
        modelBuilder.Entity<StoreImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Store)
                  .WithMany(s => s.Images)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // === TimeSlot ===
        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Capacity).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Store)
                  .WithMany(s => s.TimeSlots)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // === Service ===
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(s => s.Store)
                  .WithMany(st => st.Services)
                  .HasForeignKey(s => s.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // === ServiceOptionGroup ===
        modelBuilder.Entity<ServiceOptionGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(e => e.Service)
                  .WithMany(s => s.OptionGroups)
                  .HasForeignKey(e => e.ServiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // === ServiceOption ===
        modelBuilder.Entity<ServiceOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.OptionGroup)
                  .WithMany(g => g.Options)
                  .HasForeignKey(e => e.OptionGroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // === BookingServiceItem ===
        modelBuilder.Entity<BookingServiceItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(10, 2);

            entity.HasOne(e => e.Booking)
                  .WithMany(b => b.ServiceItems)
                  .HasForeignKey(e => e.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceOption)
                  .WithMany(o => o.BookingItems)
                  .HasForeignKey(e => e.ServiceOptionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === Pet ===
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Pets)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Species)
                  .WithMany()
                  .HasForeignKey(e => e.SpeciesId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Breed)
                  .WithMany(b => b.Pets)
                  .HasForeignKey(e => e.BreedId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === Booking ===
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingDate).IsRequired();
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasDefaultValue(BookingStatus.Pending);

            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.PlatformVoucherDiscount).HasPrecision(18, 2);
            entity.Property(e => e.StoreVoucherDiscount).HasPrecision(18, 2);

            entity.HasIndex(e => new { e.StoreId, e.BookingDate, e.TimeSlotId });

            // Relationships
            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Store)
                  .WithMany(s => s.Bookings)
                  .HasForeignKey(b => b.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.TimeSlot)
                  .WithMany(t => t.Bookings)
                  .HasForeignKey(b => b.TimeSlotId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Pet)
                  .WithMany(p => p.Bookings)
                  .HasForeignKey(b => b.PetId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ← THÊM MỚI: Voucher relationships
            entity.HasOne(b => b.PlatformVoucher)
                  .WithMany()
                  .HasForeignKey(b => b.PlatformVoucherId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(b => b.StoreVoucher)
                  .WithMany()
                  .HasForeignKey(b => b.StoreVoucherId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // === Review ===
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rating).HasPrecision(3, 1).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);

            entity.HasOne(r => r.Store)
                  .WithMany(s => s.Reviews)
                  .HasForeignKey(r => r.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.StoreId);
        });

        // === TimeSlotOverride ===
        modelBuilder.Entity<TimeSlotOverride>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.StoreId, e.Date });
            entity.HasIndex(e => new { e.StoreId, e.Date, e.TimeSlotId })
                  .HasFilter("[TimeSlotId] IS NULL");

            entity.HasOne(o => o.Store)
                  .WithMany()
                  .HasForeignKey(o => o.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.TimeSlot)
                  .WithMany()
                  .HasForeignKey(o => o.TimeSlotId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);

            // CreatedByUserId (không có navigation property)
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(o => o.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // === Species & Breed ===
        modelBuilder.Entity<Species>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Breed>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(b => b.Species)
                  .WithMany(s => s.Breeds)
                  .HasForeignKey(b => b.SpeciesId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SpeciesId, e.Name }).IsUnique();
        });

        // === StoreSpecies & StoreCategory ===
        modelBuilder.Entity<StoreSpecies>(entity =>
        {
            entity.HasKey(e => new { e.StoreId, e.SpeciesId });
            entity.HasOne(e => e.Store)
                  .WithMany(s => s.StoreSpecies)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Species)
                  .WithMany()
                  .HasForeignKey(e => e.SpeciesId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StoreCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);   // ← thêm
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<StoreCategoryMapping>(entity =>
        {
            entity.HasKey(e => new { e.StoreId, e.CategoryId });
            entity.HasOne(e => e.Store)
                  .WithMany(s => s.StoreCategories)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.StoreCategories)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Voucher Configurations ─────────────────────────────────────────────
        modelBuilder.Entity<PlatformVoucher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.MinOrderValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);

            entity.HasOne(pv => pv.CreatedByAdmin)
                  .WithMany()
                  .HasForeignKey(pv => pv.CreatedByAdminId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StoreVoucher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.StoreId, e.Code }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.MinOrderValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);

            entity.HasOne(sv => sv.Store)
                  .WithMany()
                  .HasForeignKey(sv => sv.StoreId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sv => sv.ApplicableService)
                  .WithMany()
                  .HasForeignKey(sv => sv.ApplicableServiceId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(sv => sv.ApplicableSpecies)
                  .WithMany()
                  .HasForeignKey(sv => sv.ApplicableSpeciesId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(sv => sv.CreatedByStoreOwner)
                  .WithMany()
                  .HasForeignKey(sv => sv.CreatedByStoreOwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UsedVoucher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiscountApplied).HasPrecision(18, 2);

            // === Quan hệ ===
            entity.HasOne(uv => uv.PlatformVoucher)
                  .WithMany(pv => pv.UsedVouchers)
                  .HasForeignKey(uv => uv.PlatformVoucherId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uv => uv.StoreVoucher)
                  .WithMany(sv => sv.UsedVouchers)
                  .HasForeignKey(uv => uv.StoreVoucherId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uv => uv.User)
                  .WithMany()
                  .HasForeignKey(uv => uv.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(uv => uv.Booking)
                  .WithMany()
                  .HasForeignKey(uv => uv.BookingId)
                  .OnDelete(DeleteBehavior.Restrict);

            // === SỬA Ở ĐÂY ===
            // Bỏ unique constraint để cho phép dùng cả 2 voucher cùng lúc
            entity.HasIndex(e => new { e.UserId, e.BookingId })
                  .IsUnique(false);     // ← Quan trọng: đổi thành false
        });

        // Apply configurations from assembly (nếu bạn có IEntityTypeConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }
}