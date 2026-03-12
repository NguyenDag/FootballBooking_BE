using FootballBooking_BE.Common;
using FootballBooking_BE.Models.DTOs;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IBookingService
    {
        Task<ApiResponse<BookingResponse>> CreateBookingAsync(int userId, BookingCreateRequest request);
        Task<ApiResponse<IEnumerable<BookingResponse>>> GetMyBookingsAsync(int userId);
        Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(int userId, int bookingId);
        Task<ApiResponse<bool>> CancelBookingAsync(int userId, int detailId, CancelBookingRequest request);
        Task<ApiResponse<bool>> StaffCancelBookingAsync(int staffId, int detailId, CancelBookingRequest request);
        Task<ApiResponse<bool>> BulkCancelByPitchAsync(int staffId, BulkCancelBookingRequest request);
        Task<ApiResponse<IEnumerable<AvailabilitySlot>>> GetAvailableSlotsAsync(int pitchId, DateOnly playDate);
    }
}
