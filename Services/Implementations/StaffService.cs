using FootballBooking_BE.Common;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Models.DTOs.Staff;
using FootballBooking_BE.Repositories.Interfaces;
using FootballBooking_BE.Services.Interfaces;

namespace FootballBooking_BE.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepo;
        private readonly IAuthRepository _authRepo;
        private readonly ILogger<StaffService> _logger;

        private static readonly string[] DayNames =
            { "", "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };

        public StaffService(
            IStaffRepository staffRepo,
            IAuthRepository authRepo,
            ILogger<StaffService> logger)
        {
            _staffRepo = staffRepo;
            _authRepo = authRepo;
            _logger = logger;
        }

        // ─── ADMIN: Quản lý staff ─────────────────────────────────

        public async Task<List<StaffSummaryResponse>> GetAllStaffAsync()
        {
            var staffList = await _staffRepo.GetAllStaffAsync();
            var result = new List<StaffSummaryResponse>();

            foreach (var staff in staffList)
            {
                var assignments = await _staffRepo.GetAssignmentsByStaffIdAsync(staff.UserId);
                var shifts = await _staffRepo.GetShiftsByStaffIdAsync(staff.UserId);

                result.Add(new StaffSummaryResponse
                {
                    UserId = staff.UserId,
                    FullName = staff.FullName,
                    Email = staff.Email,
                    Phone = staff.Phone,
                    IsActive = staff.IsActive,
                    TotalAssignedPitches = assignments.Count,
                    TotalShifts = shifts.Count
                });
            }

            return result;
        }

        public async Task<StaffResponse> GetStaffDetailAsync(int staffId)
        {
            var staff = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            return await BuildStaffResponseAsync(staff);
        }

        public async Task<StaffResponse> CreateStaffAsync(CreateStaffRequest request)
        {
            if (await _authRepo.EmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email đã được sử dụng.");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email.ToLower().Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Staff,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _staffRepo.CreateStaffAsync(user);
            return await BuildStaffResponseAsync(created);
        }

        public async Task<StaffResponse> UpdateStaffAsync(int staffId, UpdateStaffRequest request)
        {
            var staff = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            staff.FullName = request.FullName;
            staff.Phone = request.Phone;
            staff.IsActive = request.IsActive;

            var updated = await _staffRepo.UpdateStaffAsync(staff);
            return await BuildStaffResponseAsync(updated);
        }

        public async Task DeleteStaffAsync(int staffId)
        {
            var exists = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            await _staffRepo.DeleteStaffAsync(staffId);
        }

        // ─── ADMIN: Phân công sân ─────────────────────────────────

        public async Task<AssignedPitchResponse> AssignPitchAsync(int staffId, AssignPitchRequest request)
        {
            var staff = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            if (!staff.IsActive)
                throw new InvalidOperationException("Nhân viên đã bị vô hiệu hoá.");

            var alreadyAssigned = await _staffRepo.IsStaffAssignedToPitchAsync(staffId, request.PitchId);
            if (alreadyAssigned)
                throw new InvalidOperationException("Nhân viên đã được phân công sân này rồi.");

            var assignment = new StaffPitchAssignment
            {
                StaffId = staffId,
                PitchId = request.PitchId,
                AssignedAt = DateTime.UtcNow
            };

            var created = await _staffRepo.AssignPitchAsync(assignment);

            // Reload với navigation
            var full = await _staffRepo.GetAssignmentAsync(staffId, request.PitchId);
            return MapToAssignedPitch(full!);
        }

        public async Task UnassignPitchAsync(int staffId, int pitchId)
        {
            var assignment = await _staffRepo.GetAssignmentAsync(staffId, pitchId)
                ?? throw new KeyNotFoundException("Không tìm thấy phân công này.");

            // Xoá luôn ca làm việc liên quan đến sân này
            var shifts = await _staffRepo.GetShiftsByStaffAndPitchAsync(staffId, pitchId);
            foreach (var shift in shifts)
                await _staffRepo.DeleteShiftAsync(shift.ShiftId);

            await _staffRepo.UnassignPitchAsync(staffId, pitchId);
        }

        public async Task<List<AssignedPitchResponse>> GetAssignedPitchesAsync(int staffId)
        {
            _ = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            var assignments = await _staffRepo.GetAssignmentsByStaffIdAsync(staffId);
            return assignments.Select(MapToAssignedPitch).ToList();
        }

        // ─── ADMIN: Ca làm việc ───────────────────────────────────

        public async Task<ShiftResponse> CreateShiftAsync(int staffId, CreateShiftRequest request)
        {
            var staff = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            // Staff phải được phân công sân đó mới được tạo ca
            var isAssigned = await _staffRepo.IsStaffAssignedToPitchAsync(staffId, request.PitchId);
            if (!isAssigned)
                throw new InvalidOperationException(
                    "Nhân viên chưa được phân công sân này. Hãy phân công trước khi tạo ca.");

            if (request.StartTime >= request.EndTime)
                throw new InvalidOperationException("Giờ bắt đầu phải nhỏ hơn giờ kết thúc.");

            // Kiểm tra trùng ca (cùng staff + sân + thứ + giờ giao nhau)
            var existingShifts = await _staffRepo.GetShiftsByStaffAndPitchAsync(staffId, request.PitchId);
            var conflict = existingShifts.Any(s =>
                s.IsActive &&
                s.DayOfWeek == request.DayOfWeek &&
                s.StartTime < request.EndTime &&
                s.EndTime > request.StartTime);

            if (conflict)
                throw new InvalidOperationException(
                    $"Ca làm bị trùng với ca đã tồn tại vào {DayNames[request.DayOfWeek]}.");

            var shift = new StaffShift
            {
                StaffId = staffId,
                PitchId = request.PitchId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _staffRepo.CreateShiftAsync(shift);
            var full = await _staffRepo.GetShiftByIdAsync(created.ShiftId);
            return MapToShiftResponse(full!);
        }

        public async Task<ShiftResponse> UpdateShiftAsync(int staffId, int shiftId, UpdateShiftRequest request)
        {
            var shift = await _staffRepo.GetShiftByIdAsync(shiftId)
                ?? throw new KeyNotFoundException("Không tìm thấy ca làm việc.");

            if (shift.StaffId != staffId)
                throw new InvalidOperationException("Ca làm này không thuộc nhân viên đã chọn.");

            if (request.StartTime >= request.EndTime)
                throw new InvalidOperationException("Giờ bắt đầu phải nhỏ hơn giờ kết thúc.");

            // Kiểm tra trùng ca (bỏ qua chính ca đang sửa)
            var existingShifts = await _staffRepo.GetShiftsByStaffAndPitchAsync(staffId, shift.PitchId);
            var conflict = existingShifts.Any(s =>
                s.ShiftId != shiftId &&
                s.IsActive &&
                s.DayOfWeek == request.DayOfWeek &&
                s.StartTime < request.EndTime &&
                s.EndTime > request.StartTime);

            if (conflict)
                throw new InvalidOperationException(
                    $"Ca làm bị trùng với ca đã tồn tại vào {DayNames[request.DayOfWeek]}.");

            shift.DayOfWeek = request.DayOfWeek;
            shift.StartTime = request.StartTime;
            shift.EndTime = request.EndTime;
            shift.IsActive = request.IsActive;

            var updated = await _staffRepo.UpdateShiftAsync(shift);
            var full = await _staffRepo.GetShiftByIdAsync(updated.ShiftId);
            return MapToShiftResponse(full!);
        }

        public async Task DeleteShiftAsync(int staffId, int shiftId)
        {
            var shift = await _staffRepo.GetShiftByIdAsync(shiftId)
                ?? throw new KeyNotFoundException("Không tìm thấy ca làm việc.");

            if (shift.StaffId != staffId)
                throw new InvalidOperationException("Ca làm này không thuộc nhân viên đã chọn.");

            await _staffRepo.DeleteShiftAsync(shiftId);
        }

        public async Task<List<ShiftResponse>> GetShiftsAsync(int staffId)
        {
            _ = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            var shifts = await _staffRepo.GetShiftsByStaffIdAsync(staffId);
            return shifts.Select(MapToShiftResponse).ToList();
        }

        // ─── STAFF: Tự xem ────────────────────────────────────────

        public async Task<StaffResponse> GetMyProfileAsync(int staffId)
            => await GetStaffDetailAsync(staffId);

        public async Task<List<PendingBookingDetailResponse>> GetPendingBookingsAsync(int staffId)
        {
            _ = await _staffRepo.GetStaffByIdAsync(staffId)
                ?? throw new KeyNotFoundException("Không tìm thấy nhân viên.");

            var details = await _staffRepo.GetPendingBookingDetailsForStaffAsync(staffId);
            var result = new List<PendingBookingDetailResponse>();

            foreach (var d in details)
            {
                // Kiểm tra xem booking này có nằm trong ca làm của staff không
                var isWithinShift = await _staffRepo.IsStaffOnShiftAsync(
                    staffId,
                    d.PitchId,
                    d.PlayDate.DayOfWeek,
                    d.StartTime,
                    d.EndTime);

                result.Add(new PendingBookingDetailResponse
                {
                    DetailId = d.DetailId,
                    BookingId = d.BookingId,
                    CustomerName = d.Booking.User.FullName,
                    CustomerPhone = d.Booking.User.Phone ?? "",
                    PitchId = d.PitchId,
                    PitchName = d.Pitch.PitchName,
                    PlayDate = d.PlayDate,
                    StartTime = d.StartTime.ToString(@"hh\:mm"),
                    EndTime = d.EndTime.ToString(@"hh\:mm"),
                    DurationMinutes = d.DurationMinutes,
                    PriceAtBooking = d.PriceAtBooking,
                    DetailStatus = d.DetailStatus,
                    CreatedAt = d.CreatedAt,
                    IsWithinShift = isWithinShift
                });
            }

            return result;
        }

        // ─── STAFF: Xác nhận / Từ chối ────────────────────────────

        public async Task ConfirmBookingDetailAsync(
            int staffId,
            int detailId,
            ConfirmBookingDetailRequest request)
        {
            var detail = await _staffRepo.GetBookingDetailByIdAsync(detailId)
                ?? throw new KeyNotFoundException("Không tìm thấy booking detail.");

            // 1. Staff có được phân công sân này không?
            var isAssigned = await _staffRepo.IsStaffAssignedToPitchAsync(staffId, detail.PitchId);
            if (!isAssigned)
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền xác nhận booking tại sân này.");

            // 2. Booking có đang ở trạng thái PENDING không?
            if (detail.DetailStatus != "PENDING")
                throw new InvalidOperationException(
                    $"Booking đang ở trạng thái '{detail.DetailStatus}', không thể xác nhận.");

            // 3. Staff có trong ca làm không?
            var isOnShift = await _staffRepo.IsStaffOnShiftAsync(
                staffId,
                detail.PitchId,
                detail.PlayDate.DayOfWeek,
                detail.StartTime,
                detail.EndTime);

            if (!isOnShift)
                throw new UnauthorizedAccessException(
                    "Bạn không có ca làm tại thời điểm này. Chỉ có thể xác nhận booking trong ca của mình.");

            // 4. Cập nhật trạng thái
            detail.DetailStatus = "CONFIRMED";
            detail.StaffId = staffId;

            await _staffRepo.UpdateBookingDetailAsync(detail);

            _logger.LogInformation(
                "Staff {StaffId} confirmed BookingDetail {DetailId}", staffId, detailId);
        }

        public async Task RejectBookingDetailAsync(
            int staffId,
            int detailId,
            RejectBookingDetailRequest request)
        {
            var detail = await _staffRepo.GetBookingDetailByIdAsync(detailId)
                ?? throw new KeyNotFoundException("Không tìm thấy booking detail.");

            // 1. Staff có được phân công sân này không?
            var isAssigned = await _staffRepo.IsStaffAssignedToPitchAsync(staffId, detail.PitchId);
            if (!isAssigned)
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền từ chối booking tại sân này.");

            // 2. Chỉ được từ chối khi đang PENDING
            if (detail.DetailStatus != "PENDING")
                throw new InvalidOperationException(
                    $"Booking đang ở trạng thái '{detail.DetailStatus}', không thể từ chối.");

            // 3. Cập nhật trạng thái
            detail.DetailStatus = "CANCELLED";
            detail.CancellationReason = request.Reason;

            await _staffRepo.UpdateBookingDetailAsync(detail);

            _logger.LogInformation(
                "Staff {StaffId} rejected BookingDetail {DetailId}: {Reason}",
                staffId, detailId, request.Reason);
        }

        // ─── PRIVATE MAPPERS ──────────────────────────────────────

        private async Task<StaffResponse> BuildStaffResponseAsync(User staff)
        {
            var assignments = await _staffRepo.GetAssignmentsByStaffIdAsync(staff.UserId);
            var shifts = await _staffRepo.GetShiftsByStaffIdAsync(staff.UserId);

            return new StaffResponse
            {
                UserId = staff.UserId,
                FullName = staff.FullName,
                Email = staff.Email,
                Phone = staff.Phone,
                IsActive = staff.IsActive,
                CreatedAt = staff.CreatedAt,
                AssignedPitches = assignments.Select(MapToAssignedPitch).ToList(),
                Shifts = shifts.Select(MapToShiftResponse).ToList()
            };
        }

        private static AssignedPitchResponse MapToAssignedPitch(StaffPitchAssignment a) => new()
        {
            AssignmentId = a.Id,
            PitchId = a.PitchId,
            PitchName = a.Pitch.PitchName,
            PitchType = a.Pitch.PitchType,
            Status = a.Pitch.Status,
            AssignedAt = a.AssignedAt
        };

        private static ShiftResponse MapToShiftResponse(StaffShift s) => new()
        {
            ShiftId = s.ShiftId,
            StaffId = s.StaffId,
            StaffName = s.Staff?.FullName ?? "",
            PitchId = s.PitchId,
            PitchName = s.Pitch?.PitchName ?? "",
            DayOfWeek = s.DayOfWeek,
            DayOfWeekName = DayNames[s.DayOfWeek],
            StartTime = s.StartTime.ToString(@"hh\:mm"),
            EndTime = s.EndTime.ToString(@"hh\:mm"),
            IsActive = s.IsActive
        };
    }
}
