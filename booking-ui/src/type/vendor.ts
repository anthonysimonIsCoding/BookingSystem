// src/types/vendor.ts
export interface TopService {
    rank: number;
    serviceName: string;
    price: number;
    bookingCount: number;
}

export interface RecentReview {
    rating: number;
    comment?: string;
    customerName: string;
    petName?: string;
    createdAt: string;
}

export interface VendorDashboardData {
    totalBookings: number;
    pendingBookings: number;
    totalRevenue: number;
    averageRating: number;
    topServices: TopService[];
    recentReviews: RecentReview[];
}