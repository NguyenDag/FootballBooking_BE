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
