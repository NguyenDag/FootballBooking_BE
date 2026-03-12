using System.ComponentModel.DataAnnotations;

namespace FootballBooking_BE.Models.DTOs
{
    public class PitchDTO
    {
        public int PitchId { get; set; }
        public string PitchName { get; set; } = null!;
        public string PitchType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PriceSlotDTO> PriceSlots { get; set; } = new();
    }

    public class PriceSlotDTO
    {
        public int PriceSlotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal PricePerHour { get; set; }
        public string ApplyOn { get; set; } = "ALL";
        public bool IsPeakHour { get; set; }
    }

    public class CreatePitchRequest
    {
        [Required]
        [MaxLength(100)]
        public string PitchName { get; set; } = null!;

        [Required]
        [RegularExpression("5_PERSON|7_PERSON|11_PERSON", ErrorMessage = "Invalid pitch type")]
        public string PitchType { get; set; } = null!;

        [RegularExpression("ACTIVE|MAINTENANCE|INACTIVE", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = "ACTIVE";

        public string? Description { get; set; }

    }

    public class PriceSlotRequest
    {
        public int? PriceSlotId { get; set; } // Nếu có ID -> Update, không có ID -> Create

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerHour { get; set; }

        [RegularExpression("WEEKDAY|WEEKEND|ALL", ErrorMessage = "Invalid ApplyOn value")]
        public string ApplyOn { get; set; } = "ALL";

        public bool IsPeakHour { get; set; } = false;
    }

    public class UpdatePitchRequest
    {
        [MaxLength(100)]
        public string? PitchName { get; set; }

        [RegularExpression("5_PERSON|7_PERSON|11_PERSON", ErrorMessage = "Invalid pitch type")]
        public string? PitchType { get; set; }

        [RegularExpression("ACTIVE|MAINTENANCE|INACTIVE", ErrorMessage = "Invalid status")]
        public string? Status { get; set; }

        public string? Description { get; set; }
    }
}
