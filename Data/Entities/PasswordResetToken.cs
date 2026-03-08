using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("PasswordResetTokens")]
    public class PasswordResetToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; } = false;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
