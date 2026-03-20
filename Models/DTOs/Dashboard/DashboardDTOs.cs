using System;

namespace FootballBooking_BE.Models.DTOs.Dashboard
{
    public class DashboardStatsResponse
    {
        public int UpcomingConfirmedCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public int CompletedBookingsCount { get; set; }
        public int RejectedBookingsCount { get; set; }
        public List<UpcomingBookingDto> UpcomingBookings { get; set; } = new();
    }

    public class UpcomingBookingDto
    {
        public int DetailId { get; set; }
        public string PitchName { get; set; } = null!;
        public DateOnly PlayDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = null!;
        public string? CustomerName { get; set; } // Useful for Admin/Staff
    }

    public class AdminAdvancedStatsResponse
    {
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CancellationRate { get; set; } // percentage
        
        public List<BookingsByDateDto> BookingsByDate { get; set; } = new();
        public List<RevenueByPitchDto> RevenueByPitch { get; set; } = new();
    }

    public class BookingsByDateDto
    {
        public string DateLabel { get; set; } = null!;
        public int BookingsCount { get; set; }
    }

    public class RevenueByPitchDto
    {
        public int PitchId { get; set; }
        public string PitchName { get; set; } = null!;
        public decimal TotalRevenue { get; set; }
    }
}
