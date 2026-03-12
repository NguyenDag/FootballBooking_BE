using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("StaffShifts")]
    public class StaffShift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShiftId { get; set; }

        public int StaffId { get; set; }
        public int PitchId { get; set; }

        /// <summary>
        /// 1 = Thứ Hai, 2 = Thứ Ba, ..., 7 = Chủ Nhật
        /// Tương ứng với (int)DayOfWeek nhưng quy ước ISO (Monday=1)
        /// </summary>
        public int DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(StaffId))]
        public User Staff { get; set; } = null!;

        [ForeignKey(nameof(PitchId))]
        public Pitch Pitch { get; set; } = null!;
    }
}
