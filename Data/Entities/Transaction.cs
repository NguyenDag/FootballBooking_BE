using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        public int WalletId { get; set; }

        [Required]
        [MaxLength(30)]
        public string TransactionType { get; set; } = null!;
        // TOP_UP | BOOKING_PAYMENT | REFUND | WITHDRAWAL | ADJUSTMENT

        [Required]
        [MaxLength(10)]
        public string Direction { get; set; } = null!; // CREDIT | DEBIT

        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal BalanceBefore { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal BalanceAfter { get; set; }

        public int? BookingId { get; set; }

        [MaxLength(100)]
        public string? ReferenceId { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "SUCCESS"; // PENDING | SUCCESS | FAILED | REVERSED

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; set; } = null!;

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }

        public Payment? Payment { get; set; }
        public Refund? Refund { get; set; }
        public TopUpRequest? TopUpRequest { get; set; }
    }
}
