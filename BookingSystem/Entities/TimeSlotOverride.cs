using System;

namespace BookingSystem.Entities;

public class TimeSlotOverride
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }               // Để query nhanh theo cửa hàng
    public Guid? TimeSlotId { get; set; }           // null = áp dụng cho TOÀN BỘ NGÀY (full day)

    public DateOnly Date { get; set; }              // Ngày áp dụng override

    // Các trường override (chỉ có ý nghĩa khi TimeSlotId != null, trừ IsActive khi full day)
    public TimeSpan? StartTime { get; set; }        // null = giữ nguyên template
    public TimeSpan? EndTime { get; set; }
    public int? Capacity { get; set; }              // null = giữ nguyên, nếu set thì override
    public bool? IsActive { get; set; }             // null = giữ nguyên, true/false để bật/tắt

    // Đặc biệt cho full day closure (khi TimeSlotId == null)
    public bool IsFullDayClosure { get; set; } = false;  // true = đóng cửa hoàn toàn ngày đó

    // Thông tin bổ sung (audit & UX)
    public string? Reason { get; set; }             // "Đóng để vệ sinh", "Nhận thêm khách gấp", "Nghỉ lễ",...
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }       // Ai tạo override (chủ tiệm / nhân viên)

    // Navigation
    public Store Store { get; set; } = null!;
    public TimeSlot? TimeSlot { get; set; }         // optional, chỉ khi TimeSlotId != null
}