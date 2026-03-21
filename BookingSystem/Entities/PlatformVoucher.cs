using BookingSystem.Entities.Enums; // nếu bạn có enum cho loại giảm giá
using System;
using System.Collections.Generic;

namespace BookingSystem.Entities;

public class PlatformVoucher
{
	public Guid Id { get; set; }

	public string Code { get; set; } = null!;           // mã voucher người dùng nhập, ví dụ: "WELCOME2025", "PETLOVE30"

	public string? Name { get; set; }                   // Tên hiển thị: "Khuyến mãi chào mừng năm mới"
	public string? Description { get; set; }            // Mô tả chi tiết

	public VoucherDiscountType DiscountType { get; set; } // Percent hoặc Fixed
	public decimal DiscountValue { get; set; }          // 30 (30%) hoặc 50000 (50k)

	public decimal? MinOrderValue { get; set; }         // Đơn tối thiểu để áp dụng (null = không giới hạn)
	public decimal? MaxDiscountAmount { get; set; }     // Giới hạn giảm tối đa (ví dụ max 200k khi giảm %)

	public int? UsageLimitPerUser { get; set; }         // Số lần dùng tối đa / user (null = không giới hạn)
	public int? TotalUsageLimit { get; set; }           // Tổng số lần dùng toàn hệ thống (null = vô hạn)

	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }              // null = không hết hạn

	public bool IsActive { get; set; } = true;

	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public Guid? CreatedByAdminId { get; set; }         // Ai tạo (admin)

	// Navigation
	public User? CreatedByAdmin { get; set; }
	public ICollection<UsedVoucher> UsedVouchers { get; set; } = new List<UsedVoucher>();
}