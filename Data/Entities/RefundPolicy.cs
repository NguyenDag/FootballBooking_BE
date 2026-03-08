using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("RefundPolicies")]
    public class RefundPolicy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PolicyId { get; set; }

        public int CancelBeforeHours { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RefundPercentage { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
