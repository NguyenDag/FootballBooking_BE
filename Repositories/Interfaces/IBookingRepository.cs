using FootballBooking_BE.Data.Entities;

namespace FootballBooking_BE.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);
        Task<Booking> UpdateBookingAsync(Booking booking);
        Task<bool> CheckOverlapAsync(int pitchId, DateOnly playDate, TimeSpan startTime, TimeSpan endTime);
        Task<BookingDetail?> GetBookingDetailByIdAsync(int detailId);
        Task AddStatusHistoryAsync(BookingStatusHistory history);
        Task<IEnumerable<BookingDetail>> GetActiveBookingDetailsByPitchAsync(int pitchId, DateOnly fromDate);
        Task<bool> IsStaffAssignedToPitchAsync(int staffId, int pitchId);
        Task<IEnumerable<BookingDetail>> GetAllBookingDetailsAsync();
    }
}
