using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("StaffPitchAssignments")]
    public class StaffPitchAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int StaffId { get; set; }
        public int PitchId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(StaffId))]
        public User Staff { get; set; } = null!;

        [ForeignKey(nameof(PitchId))]
        public Pitch Pitch { get; set; } = null!;
    }
}
