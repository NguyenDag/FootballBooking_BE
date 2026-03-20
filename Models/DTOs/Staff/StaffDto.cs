using System.ComponentModel.DataAnnotations;

namespace FootballBooking_BE.Models.DTOs.Staff
{
    // ─── STAFF ACCOUNT DTOs ──────────────────────────────────────

    public class CreateStaffRequest
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Phone]
        [MaxLength(15)]
        public string? Phone { get; set; }
    }

    public class UpdateStaffRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Phone]
        [MaxLength(15)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class StaffResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<AssignedPitchResponse> AssignedPitches { get; set; } = new();
        public List<ShiftResponse> Shifts { get; set; } = new();
    }

    public class StaffSummaryResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public int TotalAssignedPitches { get; set; }
        public int TotalShifts { get; set; }
    }

    // ─── PITCH ASSIGNMENT DTOs ───────────────────────────────────

    public class AssignPitchRequest
    {
        [Required]
        public int PitchId { get; set; }
    }

    public class AssignedPitchResponse
    {
        public int AssignmentId { get; set; }
        public int PitchId { get; set; }
        public string PitchName { get; set; } = null!;
        public string PitchType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime AssignedAt { get; set; }
    }

    // ─── SHIFT DTOs ───────────────────────────────────────────────

    public class CreateShiftRequest
    {
        [Required]
        public int PitchId { get; set; }

        /// <summary>1=Thứ Hai, 2=Thứ Ba, ..., 7=Chủ Nhật</summary>
        [Required]
        [Range(1, 7, ErrorMessage = "DayOfWeek phải từ 1 (Thứ Hai) đến 7 (Chủ Nhật)")]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }

    public class UpdateShiftRequest
    {
        [Required]
        [Range(1, 7)]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ShiftResponse
    {
        public int ShiftId { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; } = null!;
        public int PitchId { get; set; }
        public string PitchName { get; set; } = null!;
        public int DayOfWeek { get; set; }
        public string DayOfWeekName { get; set; } = null!; // "Thứ Hai", "Thứ Ba"...
        public string StartTime { get; set; } = null!;     // "08:00"
        public string EndTime { get; set; } = null!;       // "17:00"
        public bool IsActive { get; set; }
    }

    // ─── BOOKING CONFIRMATION DTOs ───────────────────────────────

    public class ConfirmBookingDetailRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class RejectBookingDetailRequest
    {
        [Required(ErrorMessage = "Lý do từ chối không được để trống")]
        public string Reason { get; set; } = null!;
    }

    public class PendingBookingDetailResponse
    {
        public int DetailId { get; set; }
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public int PitchId { get; set; }
        public string PitchName { get; set; } = null!;
        public DateOnly PlayDate { get; set; }
        public string StartTime { get; set; } = null!;
        public string EndTime { get; set; } = null!;
        public int DurationMinutes { get; set; }
        public decimal PriceAtBooking { get; set; }
        public string DetailStatus { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        /// <summary>Staff có trong ca làm không (để FE hiển thị cảnh báo)</summary>
        public bool IsWithinShift { get; set; }
    }

    public class PitchScheduleSlotResponse
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
        public string? DetailStatus { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public int? BookingDetailId { get; set; }
    }
}
