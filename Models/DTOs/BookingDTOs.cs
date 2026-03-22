using System.ComponentModel.DataAnnotations;

namespace FootballBooking_BE.Models.DTOs
{
    public class BookingCreateRequest
    {
        [Required]
        public int PitchId { get; set; }

        [Required]
        public DateOnly PlayDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(60, 120)]
        public int DurationMinutes { get; set; } // 60, 90, 120

        public string? Notes { get; set; }
    }

    public class BookingResponse
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public string? Notes { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }

        public DateTime CreatedAt { get; set; }
        public List<BookingDetailResponse> Details { get; set; } = new();
    }

    public class BookingDetailResponse
    {
        public int DetailId { get; set; }
        public int PitchId { get; set; }
        public string PitchName { get; set; } = null!;
        public DateOnly PlayDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public decimal PriceAtBooking { get; set; }
        public string Status { get; set; } = null!;
        public string? CancellationReason { get; set; }
    }

    public class CancelBookingRequest
    {
        [Required]
        public string Reason { get; set; } = null!;
    }

    public class BulkCancelBookingRequest
    {
        [Required]
        public int PitchId { get; set; }

        [Required]
        public DateOnly FromDate { get; set; }

        [Required]
        public string Reason { get; set; } = null!;
    }

    public class AvailabilitySlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
    }
}
