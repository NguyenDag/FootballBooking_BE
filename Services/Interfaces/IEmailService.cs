namespace FootballBooking_BE.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp);
    }
}
