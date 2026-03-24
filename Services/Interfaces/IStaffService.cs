using FootballBooking_BE.Models.DTOs.Staff;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IStaffService
    {
        // ─── ADMIN: Quản lý staff ─────────────────────────────────
        Task<List<StaffSummaryResponse>> GetAllStaffAsync();
        Task<StaffResponse> GetStaffDetailAsync(int staffId);
        Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request);
        Task<StaffResponse> UpdateStaffAsync(int staffId, UpdateStaffRequest request);
        Task DeleteStaffAsync(int staffId); // soft delete
        Task HardDeleteStaffAsync(int staffId); // hard delete (xóa hẳn DB)

        // ─── ADMIN: Phân công sân ─────────────────────────────────
        Task<AssignedPitchResponse> AssignPitchAsync(int staffId, AssignPitchRequest request);
        Task UnassignPitchAsync(int staffId, int pitchId);
        Task<List<AssignedPitchResponse>> GetAssignedPitchesAsync(int staffId);

        // ─── ADMIN: Ca làm việc ───────────────────────────────────
        Task<ShiftResponse> CreateShiftAsync(int staffId, CreateShiftRequest request);
        Task<ShiftResponse> UpdateShiftAsync(int staffId, int shiftId, UpdateShiftRequest request);
        Task DeleteShiftAsync(int staffId, int shiftId);
        Task<List<ShiftResponse>> GetShiftsAsync(int staffId);

        // ─── STAFF: Tự xem thông tin ──────────────────────────────
        Task<StaffResponse> GetMyProfileAsync(int staffId);
        Task<List<PendingBookingDetailResponse>> GetPendingBookingsAsync(int staffId);
        Task<List<PitchScheduleSlotResponse>> GetPitchScheduleAsync(int staffId, int pitchId, DateOnly date);

        // ─── STAFF: Xác nhận / từ chối booking ───────────────────
        Task ConfirmBookingDetailAsync(int staffId, int detailId, ConfirmBookingDetailRequest request);
        Task RejectBookingDetailAsync(int staffId, int detailId, RejectBookingDetailRequest request);
    }
}
