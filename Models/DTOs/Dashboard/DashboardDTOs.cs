using System;

namespace FootballBooking_BE.Models.DTOs.Dashboard
{
    public class DashboardStatsResponse
    {
        public int UpcomingConfirmedCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public int CompletedBookingsCount { get; set; }
        public int RejectedBookingsCount { get; set; }
        public int PendingBookingsCount { get; set; }
        public int TotalManagedPitches { get; set; }
        public List<UpcomingBookingDto> UpcomingBookings { get; set; } = new();
        public List<UpcomingBookingDto> PendingBookings { get; set; } = new();
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
        public string? CustomerPhone { get; set; }

    }

    public class AdminAdvancedStatsResponse
    {
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CancellationRate { get; set; } // percentage
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        
        public List<BookingsByDateDto> BookingsByDate { get; set; } = new();
        public List<RevenueByPitchDto> RevenueByPitch { get; set; } = new();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
        public List<PeakHourDto> PeakHours { get; set; } = new();
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

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = null!; // e.g., "T1", "T2" or "Month YYYY"
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
    }

    public class PeakHourDto
    {
        public string HourRange { get; set; } = null!; // e.g., "06:00-09:00"
        public int BookingsCount { get; set; }
    }
}
