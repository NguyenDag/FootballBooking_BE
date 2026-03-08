using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("Refunds")]
    public class Refund
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RefundId { get; set; }

        public int PaymentId { get; set; }
        public int? BookingDetailId { get; set; }
        public int? TransactionId { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal RefundAmount { get; set; }

        [Required]
        public string Reason { get; set; } = null!;

        [MaxLength(20)]
        public string Status { get; set; } = "PENDING";
        // PENDING | APPROVED | PROCESSING | COMPLETED | REJECTED

        [MaxLength(30)]
        public string? RefundMethod { get; set; }
        // WALLET | BANK_TRANSFER | ORIGINAL_METHOD

        // Bank info (nếu hoàn qua chuyển khoản)
        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(100)]
        public string? BankAccountName { get; set; }

        public int RequestedBy { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }

        [MaxLength(200)]
        public string? GatewayRefundId { get; set; }

        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; } = null!;

        [ForeignKey(nameof(BookingDetailId))]
        public BookingDetail? BookingDetail { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction? Transaction { get; set; }

        [ForeignKey(nameof(RequestedBy))]
        public User RequestedByUser { get; set; } = null!;

        [ForeignKey(nameof(ReviewedBy))]
        public User? ReviewedByUser { get; set; }
    }
}
