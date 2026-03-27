namespace FootballBooking_BE.Models
{
    public enum PaymentProcessResult
    {
        Skipped,
        TopUpOnly,
        BookingPaid,
        AlreadyPaid,
        AlreadyProcessed,
        Error
    }

    public class SyncResult
    {
        public int TotalProcessed { get; set; }
        public List<int> PaidBookingIds { get; set; } = new();
        public List<int> PartiallyPaidBookingIds { get; set; } = new();
    }
}
