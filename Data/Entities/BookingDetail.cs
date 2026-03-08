using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("BookingDetails")]
    public class BookingDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DetailId { get; set; }

        public int BookingId { get; set; }
        public int PitchId { get; set; }
        public int? StaffId { get; set; }

        public DateOnly PlayDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int DurationMinutes { get; set; } // 60 | 90 | 120

        [Column(TypeName = "decimal(15,2)")]
        public decimal PriceAtBooking { get; set; }

        [MaxLength(20)]
        public string DetailStatus { get; set; } = "PENDING"; // PENDING | CONFIRMED | CANCELLED | COMPLETED

        public string? CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;

        [ForeignKey(nameof(PitchId))]
        public Pitch Pitch { get; set; } = null!;

        [ForeignKey(nameof(StaffId))]
        public User? Staff { get; set; }

        public ICollection<BookingStatusHistory> StatusHistories { get; set; } = new List<BookingStatusHistory>();
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
