namespace BookingSystem.Entities.Enums;

public enum BookingStatus : byte
{
    Pending = 0,          // mới đặt
    Received = 1,         // đã nhận pet
    Caring = 2,           // đang chăm sóc
    WaitingPickup = 3,    // xong, đợi đón
    Completed = 4,        // đã đón
    Cancelled = 5         // bị cancel
}