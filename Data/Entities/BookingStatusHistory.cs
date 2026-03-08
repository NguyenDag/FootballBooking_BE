using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("BookingStatusHistory")]
    public class BookingStatusHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BookingDetailId { get; set; }

        [MaxLength(20)]
        public string? OldStatus { get; set; }

        [MaxLength(20)]
        public string? NewStatus { get; set; }

        public int? ChangedBy { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(BookingDetailId))]
        public BookingDetail BookingDetail { get; set; } = null!;

        [ForeignKey(nameof(ChangedBy))]
        public User? ChangedByUser { get; set; }
    }
}
