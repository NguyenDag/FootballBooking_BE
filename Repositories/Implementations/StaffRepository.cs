using FootballBooking_BE.Data;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballBooking_BE.Repositories.Implementations
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AppDbContext _context;

        public StaffRepository(AppDbContext context)
        {
            _context = context;
        }

        // ─── STAFF ACCOUNT ────────────────────────────────────────

        public async Task<List<User>> GetAllStaffAsync()
            => await _context.Users
                .Where(u => u.Role == "STAFF")
                .OrderBy(u => u.FullName)
                .ToListAsync();

        public async Task<User?> GetStaffByIdAsync(int staffId)
            => await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == staffId && u.Role == "STAFF");

        public async Task<User> CreateStaffAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateStaffAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteStaffAsync(int staffId)
        {
            var staff = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == staffId && u.Role == "STAFF");

            if (staff == null) return false;

            staff.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HardDeleteStaffAsync(int staffId)
        {
            var staff = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == staffId && u.Role == "STAFF");

            if (staff == null) return false;

            // 1. Xóa các bảng phụ thuộc trực tiếp (Restrict)
            var shifts = await _context.StaffShifts.Where(s => s.StaffId == staffId).ToListAsync();
            _context.StaffShifts.RemoveRange(shifts);

            var assignments = await _context.StaffPitchAssignments.Where(a => a.StaffId == staffId).ToListAsync();
            _context.StaffPitchAssignments.RemoveRange(assignments);

            var refreshTokens = await _context.RefreshTokens.Where(rt => rt.UserId == staffId).ToListAsync();
            _context.RefreshTokens.RemoveRange(refreshTokens);

            var resetTokens = await _context.PasswordResetTokens.Where(t => t.UserId == staffId).ToListAsync();
            _context.PasswordResetTokens.RemoveRange(resetTokens);

            // 2. Xóa các bảng mà Staff đóng vai trò là Customer (UserId)
            // Tìm tất cả các Bookings của staff (nếu có)
            var userBookingIds = await _context.Bookings
                .Where(b => b.UserId == staffId)
                .Select(b => b.BookingId)
                .ToListAsync();

            if (userBookingIds.Any())
            {
                // Xóa Refunds liên quan đến các Payments của các Bookings này
                var refundsToDelete = await _context.Refunds
                    .Where(r => _context.Payments.Any(p => p.PaymentId == r.PaymentId && userBookingIds.Contains(p.BookingId)))
                    .ToListAsync();
                _context.Refunds.RemoveRange(refundsToDelete);

                // Xóa các Refunds mà Staff là người yêu cầu (RequestedBy)
                var requestedRefunds = await _context.Refunds.Where(r => r.RequestedBy == staffId).ToListAsync();
                _context.Refunds.RemoveRange(requestedRefunds);

                // Xóa Payments liên quan đến các Bookings này
                var paymentsToDelete = await _context.Payments
                    .Where(p => userBookingIds.Contains(p.BookingId))
                    .ToListAsync();
                _context.Payments.RemoveRange(paymentsToDelete);

                // Xóa các Transactions liên quan đến Bookings này trước
                var bookingTransactions = await _context.Transactions
                    .Where(t => t.BookingId != null && userBookingIds.Contains(t.BookingId.Value))
                    .ToListAsync();
                _context.Transactions.RemoveRange(bookingTransactions);

                // Xóa chính các Bookings (sẽ cascade xóa BookingDetails)
                var bookingsToDelete = await _context.Bookings
                    .Where(b => userBookingIds.Contains(b.BookingId))
                    .ToListAsync();
                _context.Bookings.RemoveRange(bookingsToDelete);
            }

            // Xóa TopUpRequests, Wallet và các Transactions của Wallet (Restrict)
            var topUps = await _context.TopUpRequests.Where(t => t.UserId == staffId).ToListAsync();
            _context.TopUpRequests.RemoveRange(topUps);

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == staffId);
            if (wallet != null)
            {
                // QUAN TRỌNG: Xóa tất cả giao dịch trong ví trước khi xóa ví (tránh lỗi Restrict)
                var walletTransactions = await _context.Transactions
                    .Where(t => t.WalletId == wallet.WalletId)
                    .ToListAsync();
                _context.Transactions.RemoveRange(walletTransactions);

                _context.Wallets.Remove(wallet);
            }

            // 3. Nullify các bảng mà Staff đóng vai trò là Admin/Reviewer (StaffId/ConfirmedBy/ReviewedBy)
            var managedDetails = await _context.BookingDetails.Where(bd => bd.StaffId == staffId).ToListAsync();
            foreach (var bd in managedDetails) bd.StaffId = null;

            var confirmedPayments = await _context.Payments.Where(p => p.ConfirmedBy == staffId).ToListAsync();
            foreach (var p in confirmedPayments) p.ConfirmedBy = null;

            var reviewedRefunds = await _context.Refunds.Where(r => r.ReviewedBy == staffId).ToListAsync();
            foreach (var r in reviewedRefunds) r.ReviewedBy = null;

            var confirmedTopUps = await _context.TopUpRequests.Where(t => t.ConfirmedBy == staffId).ToListAsync();
            foreach (var t in confirmedTopUps) t.ConfirmedBy = null;

            var history = await _context.BookingStatusHistories.Where(h => h.ChangedBy == staffId).ToListAsync();
            foreach (var h in history) h.ChangedBy = null;

            _context.Users.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── PITCH ASSIGNMENT ─────────────────────────────────────

        public async Task<List<StaffPitchAssignment>> GetAssignmentsByStaffIdAsync(int staffId)
            => await _context.StaffPitchAssignments
                .Include(a => a.Pitch)
                .Where(a => a.StaffId == staffId)
                .ToListAsync();

        public async Task<StaffPitchAssignment?> GetAssignmentAsync(int staffId, int pitchId)
            => await _context.StaffPitchAssignments
                .Include(a => a.Pitch)
                .FirstOrDefaultAsync(a => a.StaffId == staffId && a.PitchId == pitchId);

        public async Task<StaffPitchAssignment> AssignPitchAsync(StaffPitchAssignment assignment)
        {
            _context.StaffPitchAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> UnassignPitchAsync(int staffId, int pitchId)
        {
            var assignment = await _context.StaffPitchAssignments
                .FirstOrDefaultAsync(a => a.StaffId == staffId && a.PitchId == pitchId);

            if (assignment == null) return false;

            _context.StaffPitchAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsStaffAssignedToPitchAsync(int staffId, int pitchId)
            => await _context.StaffPitchAssignments
                .AnyAsync(a => a.StaffId == staffId && a.PitchId == pitchId);

        // ─── SHIFTS ───────────────────────────────────────────────

        public async Task<List<StaffShift>> GetShiftsByStaffIdAsync(int staffId)
            => await _context.StaffShifts
                .Include(s => s.Pitch)
                .Where(s => s.StaffId == staffId)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

        public async Task<List<StaffShift>> GetShiftsByStaffAndPitchAsync(int staffId, int pitchId)
            => await _context.StaffShifts
                .Include(s => s.Pitch)
                .Where(s => s.StaffId == staffId && s.PitchId == pitchId)
                .OrderBy(s => s.DayOfWeek)
                .ToListAsync();

        public async Task<List<StaffShift>> GetShiftsByPitchAsync(int pitchId)
            => await _context.StaffShifts
                .Include(s => s.Staff)
                .Where(s => s.PitchId == pitchId)
                .OrderBy(s => s.DayOfWeek)
                .ToListAsync();

        public async Task<StaffShift?> GetShiftByIdAsync(int shiftId)
            => await _context.StaffShifts
                .Include(s => s.Staff)
                .Include(s => s.Pitch)
                .FirstOrDefaultAsync(s => s.ShiftId == shiftId);

        public async Task<StaffShift> CreateShiftAsync(StaffShift shift)
        {
            _context.StaffShifts.Add(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<StaffShift> UpdateShiftAsync(StaffShift shift)
        {
            _context.StaffShifts.Update(shift);
            await _context.SaveChangesAsync();
            return shift;
        }

        public async Task<bool> DeleteShiftAsync(int shiftId)
        {
            var shift = await _context.StaffShifts.FindAsync(shiftId);
            if (shift == null) return false;

            _context.StaffShifts.Remove(shift);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsStaffOnShiftAsync(
            int staffId,
            int pitchId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            // Convert .NET DayOfWeek (0=Sunday) → ISO (1=Monday...7=Sunday)
            var isoDayOfWeek = dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

            return await _context.StaffShifts
                .AnyAsync(s =>
                    s.StaffId == staffId &&
                    s.PitchId == pitchId &&
                    s.IsActive == true &&
                    s.DayOfWeek == isoDayOfWeek &&
                    s.StartTime <= startTime &&
                    s.EndTime >= endTime);
        }

        // ─── BOOKING ──────────────────────────────────────────────

        public async Task<List<BookingDetail>> GetPendingBookingDetailsForStaffAsync(int staffId)
        {
            // Lấy tất cả sân mà staff được phân công
            var assignedPitchIds = await _context.StaffPitchAssignments
                .Where(a => a.StaffId == staffId)
                .Select(a => a.PitchId)
                .ToListAsync();

            return await _context.BookingDetails
                .Include(d => d.Booking)
                    .ThenInclude(b => b.User)
                .Include(d => d.Pitch)
                .Where(d =>
                    assignedPitchIds.Contains(d.PitchId) &&
                    d.DetailStatus == "PENDING")
                .OrderBy(d => d.PlayDate)
                .ThenBy(d => d.StartTime)
                .ToListAsync();
        }

        public async Task<BookingDetail?> GetBookingDetailByIdAsync(int detailId)
            => await _context.BookingDetails
                .Include(d => d.Booking)
                    .ThenInclude(b => b.User)
                .Include(d => d.Pitch)
                .FirstOrDefaultAsync(d => d.DetailId == detailId);

        public async Task<BookingDetail> UpdateBookingDetailAsync(BookingDetail detail)
        {
            _context.BookingDetails.Update(detail);
            await _context.SaveChangesAsync();
            return detail;
        }

        public async Task<Booking> UpdateBookingAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        // ─── PITCH & BOOKING (hỗ trợ GetPitchScheduleAsync) ────────
        
        public async Task<List<PriceSlot>> GetPriceSlotsByPitchIdAsync(int pitchId)
        {
            return await _context.PriceSlots
                .Where(ps => ps.PitchId == pitchId)
                .OrderBy(ps => ps.StartTime)
                .ToListAsync();
        }

        public async Task<List<BookingDetail>> GetActiveBookingDetailsForPitchAndDateAsync(int pitchId, DateOnly date)
        {
            return await _context.BookingDetails
                .Include(d => d.Booking)
                    .ThenInclude(b => b.User)
                .Where(d => 
                    d.PitchId == pitchId &&
                    d.PlayDate == date &&
                    d.DetailStatus != "CANCELLED" &&
                    d.DetailStatus != "REJECTED")
                .OrderBy(d => d.StartTime)
                .ToListAsync();
        }
    }
}
