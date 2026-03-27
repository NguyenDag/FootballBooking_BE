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
        Task<List<int>> GetStaffAssignedPitchIdsAsync(int staffId);
        Task<IEnumerable<BookingDetail>> GetAllBookingDetailsAsync();
        Task<IEnumerable<BookingDetail>> GetBookingDetailsByStaffAsync(int staffId);
        Task<IEnumerable<BookingDetail>> GetBookingDetailsByStaffAndDateAsync(int staffId, DateOnly date);
        Task<IEnumerable<BookingDetail>> GetPendingBookingDetailsByStaffAsync(int staffId);
        
        // Refund/Wallet related
        Task<IEnumerable<RefundPolicy>> GetActiveRefundPoliciesAsync();
        Task<Wallet?> GetWalletByUserIdAsync(int userId);
        Task UpdateWalletAsync(Wallet wallet);
        Task CreateTransactionAsync(Transaction transaction);
        Task CreateRefundAsync(Refund refund);
    }
}
