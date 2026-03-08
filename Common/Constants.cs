namespace FootballBooking_BE.Common
{
    public static class UserRole
    {
        public const string Admin = "ADMIN";
        public const string Staff = "STAFF";
        public const string Customer = "CUSTOMER";
    }

    public static class BookingStatus
    {
        public const string Pending = "PENDING";
        public const string Confirmed = "CONFIRMED";
        public const string Completed = "COMPLETED";
        public const string Cancelled = "CANCELLED";
        public const string Rejected = "REJECTED";
    }

    public static class PaymentStatus
    {
        public const string Unpaid = "UNPAID";
        public const string Paid = "PAID";
        public const string Refunded = "REFUNDED";
    }
}
