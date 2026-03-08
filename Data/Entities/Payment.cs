using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        public int BookingId { get; set; }
        public int? TransactionId { get; set; }

        [Required]
        [MaxLength(30)]
        public string PaymentMethod { get; set; } = null!;
        // WALLET | CASH | BANK_TRANSFER | VNPAY | MOMO | ZALOPAY

        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string? GatewayTransactionId { get; set; }

        [MaxLength(50)]
        public string? GatewayResponseCode { get; set; }

        public string? GatewayResponseData { get; set; } // JSON raw

        [MaxLength(20)]
        public string Status { get; set; } = "PENDING";
        // PENDING | SUCCESS | FAILED | CANCELLED | REFUNDED | PARTIALLY_REFUNDED

        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ConfirmedBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? Note { get; set; }

        // Navigation properties
        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; } = null!;

        [ForeignKey(nameof(TransactionId))]
        public Transaction? Transaction { get; set; }

        [ForeignKey(nameof(ConfirmedBy))]
        public User? ConfirmedByUser { get; set; }

        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
