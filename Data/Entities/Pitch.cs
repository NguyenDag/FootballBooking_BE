using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("Pitches")]
    public class Pitch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PitchId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PitchName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string PitchType { get; set; } = null!; // 5_PERSON | 7_PERSON | 11_PERSON

        [MaxLength(20)]
        public string Status { get; set; } = "ACTIVE"; // ACTIVE | MAINTENANCE | INACTIVE

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<StaffPitchAssignment> StaffPitchAssignments { get; set; } = new List<StaffPitchAssignment>();
        public ICollection<PriceSlot> PriceSlots { get; set; } = new List<PriceSlot>();
        public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    }
}
