using System.ComponentModel.DataAnnotations;

namespace FootballBooking_BE.Models.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = null!;
    }
}
