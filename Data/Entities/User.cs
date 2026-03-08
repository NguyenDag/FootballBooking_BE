using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballBooking_BE.Data.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = null!; // ADMIN | STAFF | CUSTOMER

        [MaxLength(15)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Wallet? Wallet { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<BookingDetail> AssignedBookingDetails { get; set; } = new List<BookingDetail>();
        public ICollection<StaffPitchAssignment> StaffPitchAssignments { get; set; } = new List<StaffPitchAssignment>();
        public ICollection<BookingStatusHistory> BookingStatusHistories { get; set; } = new List<BookingStatusHistory>();
        public ICollection<Payment> ConfirmedPayments { get; set; } = new List<Payment>();
        public ICollection<Refund> RequestedRefunds { get; set; } = new List<Refund>();
        public ICollection<Refund> ReviewedRefunds { get; set; } = new List<Refund>();
        public ICollection<TopUpRequest> TopUpRequests { get; set; } = new List<TopUpRequest>();
        public ICollection<TopUpRequest> ConfirmedTopUps { get; set; } = new List<TopUpRequest>();
    }
}
