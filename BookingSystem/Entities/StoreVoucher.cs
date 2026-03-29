using BookingSystem.Entities.Enums;
using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class StoreVoucher
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }                   // Voucher thuộc cửa hàng nào

    public string Code { get; set; } = null!;           // Mã riêng của store, ví dụ: "PETCARE10", "GROOMING20"

    public string? Name { get; set; }
    public string? Description { get; set; }

    public VoucherDiscountType DiscountType { get; set; } // Percent / Fixed
    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public int? UsageLimitPerUser { get; set; }         // Số lần / user
    public int? TotalUsageLimit { get; set; }           // Tổng số lần dùng cho voucher này

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Một số store có thể giới hạn voucher chỉ áp dụng cho dịch vụ nhất định
    public Guid? ApplicableServiceId { get; set; }      // null = áp dụng tất cả dịch vụ của store
    public Guid? ApplicableSpeciesId { get; set; }      // null = tất cả loài thú

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedByStoreOwnerId { get; set; }    // Ai tạo (thường là owner hoặc staff của store)

    // Navigation
    public Store Store { get; set; } = null!;
    public Service? ApplicableService { get; set; }
    public Species? ApplicableSpecies { get; set; }
    public User? CreatedByStoreOwner { get; set; }
    public ICollection<UsedVoucher> UsedVouchers { get; set; } = new List<UsedVoucher>();
}