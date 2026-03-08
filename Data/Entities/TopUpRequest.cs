using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("TopUpRequests")]
    public class TopUpRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TopupId { get; set; }

        public int UserId { get; set; }
        public int WalletId { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(30)]
        public string PaymentMethod { get; set; } = null!;
        // BANK_TRANSFER | VNPAY | MOMO | ZALOPAY

        [MaxLength(20)]
        public string Status { get; set; } = "PENDING";
        // PENDING | SUCCESS | FAILED | CANCELLED

        [MaxLength(500)]
        public string? ProofImageUrl { get; set; }

        [MaxLength(100)]
        public string? ReferenceCode { get; set; }

        [MaxLength(200)]
        public string? GatewayTransactionId { get; set; }

        public int? TransactionId { get; set; }
        public int? ConfirmedBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; set; } = null!;

        [ForeignKey(nameof(TransactionId))]
        public Transaction? Transaction { get; set; }

        [ForeignKey(nameof(ConfirmedBy))]
        public User? ConfirmedByUser { get; set; }
    }
}
