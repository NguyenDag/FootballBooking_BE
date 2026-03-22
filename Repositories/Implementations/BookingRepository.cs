using FootballBooking_BE.Data;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballBooking_BE.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Pitch)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Pitch)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking> UpdateBookingAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> CheckOverlapAsync(int pitchId, DateOnly playDate, TimeSpan startTime, TimeSpan endTime)
        {
            return await _context.BookingDetails
                .AnyAsync(d => d.PitchId == pitchId && 
                               d.PlayDate == playDate &&
                               d.DetailStatus != "CANCELLED" &&
                               d.DetailStatus != "REJECTED" &&
                               ((startTime >= d.StartTime && startTime < d.EndTime) ||
                                (endTime > d.StartTime && endTime <= d.EndTime) ||
                                (startTime <= d.StartTime && endTime >= d.EndTime)));
        }

        public async Task<BookingDetail?> GetBookingDetailByIdAsync(int detailId)
        {
            return await _context.BookingDetails
                .Include(d => d.Booking)
                .FirstOrDefaultAsync(d => d.DetailId == detailId);
        }

        public async Task AddStatusHistoryAsync(BookingStatusHistory history)
        {
            _context.BookingStatusHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BookingDetail>> GetActiveBookingDetailsByPitchAsync(int pitchId, DateOnly fromDate)
        {
            return await _context.BookingDetails
                .Include(d => d.Booking)
                .Where(d => d.PitchId == pitchId && 
                            d.PlayDate >= fromDate && 
                            d.DetailStatus != "CANCELLED" && 
                            d.DetailStatus != "COMPLETED" &&
                            d.DetailStatus != "REJECTED")
                .ToListAsync();
        }

        public async Task<bool> IsStaffAssignedToPitchAsync(int staffId, int pitchId)
        {
            return await _context.StaffPitchAssignments
                .AnyAsync(a => a.StaffId == staffId && a.PitchId == pitchId);
        }

        public async Task<List<int>> GetStaffAssignedPitchIdsAsync(int staffId)
        {
            return await _context.StaffPitchAssignments
                .Where(a => a.StaffId == staffId)
                .Select(a => a.PitchId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingDetail>> GetAllBookingDetailsAsync()
        {
            return await _context.BookingDetails
                .Include(d => d.Pitch)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.User)
                .OrderByDescending(d => d.PlayDate)
                    .ThenByDescending(d => d.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingDetail>> GetBookingDetailsByStaffAsync(int staffId)
        {
            return await _context.BookingDetails
                .Include(d => d.Pitch)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.User)
                .Where(d => d.Pitch.StaffPitchAssignments.Any(a => a.StaffId == staffId))
                .OrderByDescending(d => d.PlayDate)
                    .ThenByDescending(d => d.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingDetail>> GetBookingDetailsByStaffAndDateAsync(int staffId, DateOnly date)
        {
            return await _context.BookingDetails
                .Include(d => d.Pitch)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.User)
                .Where(d => d.Pitch.StaffPitchAssignments.Any(a => a.StaffId == staffId) && d.PlayDate == date)
                .OrderByDescending(d => d.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingDetail>> GetPendingBookingDetailsByStaffAsync(int staffId)
        {
            return await _context.BookingDetails
                .Include(d => d.Pitch)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.User)
                .Where(d => d.Pitch.StaffPitchAssignments.Any(a => a.StaffId == staffId) && d.DetailStatus == "PENDING")
                .OrderByDescending(d => d.PlayDate)
                    .ThenByDescending(d => d.StartTime)
                .ToListAsync();
        }
    }
}
