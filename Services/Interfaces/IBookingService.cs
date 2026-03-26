using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IBookingService
    {
        Task<ApiResponse<BookingResponse>> CreateBookingAsync(int userId, BookingCreateRequest request);
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetMyBookingsAsync(int userId);
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetBookingHistoryAsync(int userId);
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetUpcomingBookingsAsync(int userId);
        Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(int userId, int bookingId);
        Task<ApiResponse<bool>> CancelBookingAsync(int userId, int detailId, CancelBookingRequest request);
        Task<ApiResponse<bool>> StaffCancelBookingAsync(int staffId, string role, int detailId, CancelBookingRequest request);
        Task<ApiResponse<bool>> StaffConfirmBookingAsync(int staffId, string role, int detailId);
        Task<ApiResponse<bool>> BulkCancelByPitchAsync(int staffId, string role, BulkCancelBookingRequest request);
        Task<ApiResponse<IEnumerable<AvailabilitySlot>>> GetAvailableSlotsAsync(int pitchId, DateOnly playDate);
        Task<ApiResponse<Models.DTOs.Dashboard.DashboardStatsResponse>> GetDashboardStatsAsync(int userId, string role);
        Task<ApiResponse<Models.DTOs.Dashboard.AdminAdvancedStatsResponse>> GetAdminAdvancedStatsAsync(DateOnly fromDate, DateOnly toDate);
        
        // Staff/Admin special views
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetStaffBookingsByDateAsync(int staffId, string role, DateOnly date);
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetStaffPendingBookingsAsync(int staffId, string role);
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetStaffAllBookingsAsync(int staffId, string role, DateOnly? date = null);
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetPitchBookingsByDateAsync(int pitchId, DateOnly date);
    }
}
