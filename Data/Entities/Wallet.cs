using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("Wallets")]
    public class Wallet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WalletId { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal Balance { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<TopUpRequest> TopUpRequests { get; set; } = new List<TopUpRequest>();
    }
}
