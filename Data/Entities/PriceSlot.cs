using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("PriceSlots")]
    public class PriceSlot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PriceSlotId { get; set; }

        public int PitchId { get; set; }

        [Required]
        [MaxLength(20)]
        public string PitchType { get; set; } = null!; // 5_PERSON | 7_PERSON | 11_PERSON

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerHour { get; set; }

        [MaxLength(20)]
        public string ApplyOn { get; set; } = "ALL"; // WEEKDAY | WEEKEND | ALL

        public bool IsPeakHour { get; set; } = false;

        // Navigation properties
        [ForeignKey(nameof(PitchId))]
        public Pitch Pitch { get; set; } = null!;
    }
}
