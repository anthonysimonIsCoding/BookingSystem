namespace BookingSystem.Entities.Enums;

public enum StoreStatus
{
	Pending = 0,        // Chờ admin duyệt (mặc định khi store mới tạo)
	Approved = 1,       // Đã duyệt → được bán hàng, hiển thị dịch vụ, cho phép booking
	Rejected = 2,       // Bị từ chối
	Suspended = 3,      // Tạm khóa (admin có thể khóa sau này)
						// Có thể thêm sau: Deactivated, UnderReview...
}