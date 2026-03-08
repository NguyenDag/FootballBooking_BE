using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("Bookings")]
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [MaxLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING | CONFIRMED | COMPLETED | CANCELLED | REJECTED

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "UNPAID"; // UNPAID | PAID | REFUNDED

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
