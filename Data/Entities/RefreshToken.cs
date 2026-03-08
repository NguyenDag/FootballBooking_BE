using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Token { get; set; } = null!;          // Giá trị refresh token (hash)

        public DateTime ExpiresAt { get; set; }              // Hết hạn khi nào
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRevoked { get; set; } = false;         // Đã thu hồi chưa
        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByToken { get; set; }         // Token mới thay thế (rotation)
        public string? CreatedByIp { get; set; }             // IP tạo token
        public string? RevokedByIp { get; set; }             // IP thu hồi token

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
