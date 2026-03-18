using FootballBooking_BE.Data.Entities;

namespace FootballBooking_BE.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        // ─── STAFF ACCOUNT ────────────────────────────────────────
        Task<List<User>> GetAllStaffAsync();
        Task<User?> GetStaffByIdAsync(int staffId);
        Task<User> CreateStaffAsync(User user);
        Task<User> UpdateStaffAsync(User user);
        Task<bool> DeleteStaffAsync(int staffId); // soft delete (IsActive = false)

        // ─── PITCH ASSIGNMENT ─────────────────────────────────────
        Task<List<StaffPitchAssignment>> GetAssignmentsByStaffIdAsync(int staffId);
        Task<StaffPitchAssignment?> GetAssignmentAsync(int staffId, int pitchId);
        Task<StaffPitchAssignment> AssignPitchAsync(StaffPitchAssignment assignment);
        Task<bool> UnassignPitchAsync(int staffId, int pitchId);
        Task<bool> IsStaffAssignedToPitchAsync(int staffId, int pitchId);

        // ─── SHIFTS ───────────────────────────────────────────────
        Task<List<StaffShift>> GetShiftsByStaffIdAsync(int staffId);
        Task<List<StaffShift>> GetShiftsByStaffAndPitchAsync(int staffId, int pitchId);
        Task<StaffShift?> GetShiftByIdAsync(int shiftId);
        Task<StaffShift> CreateShiftAsync(StaffShift shift);
        Task<StaffShift> UpdateShiftAsync(StaffShift shift);
        Task<bool> DeleteShiftAsync(int shiftId);

        /// <summary>
        /// Kiểm tra staff có ca làm tại sân đó, vào đúng thứ và trong giờ hay không
        /// </summary>
        Task<bool> IsStaffOnShiftAsync(int staffId, int pitchId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime);

        // ─── BOOKING (dành cho staff xem & xác nhận) ──────────────
        Task<List<BookingDetail>> GetPendingBookingDetailsForStaffAsync(int staffId);
        Task<BookingDetail?> GetBookingDetailByIdAsync(int detailId);
        Task<BookingDetail> UpdateBookingDetailAsync(BookingDetail detail);
        Task<Booking> UpdateBookingAsync(Booking booking);
    }
}
