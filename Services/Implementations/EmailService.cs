using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using FootballBooking_BE.Models;
using FootballBooking_BE.Services.Interfaces;

namespace FootballBooking_BE.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _email;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailOptions, ILogger<EmailService> logger)
        {
            _email = emailOptions.Value;
            _logger = logger;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_email.FromName, _email.FromAddress));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Mã xác thực OTP của bạn - Football Booking";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"""
                <div style="font-family: Arial, sans-serif; max-width: 480px; margin: auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px;">
                    <h2 style="color: #10B981; text-align: center;">Football Booking System</h2>
                    <p>Xin chào,</p>
                    <p>Mã OTP để đặt lại mật khẩu của bạn là:</p>
                    <div style="font-size: 32px; font-weight: bold; letter-spacing: 5px;
                                text-align: center; padding: 15px; background: #ECFDF5;
                                border-radius: 8px; color: #059669;">
                        {otp}
                    </div>
                    <p style="margin-top: 20px; color: #666; font-size: 14px;">
                        Mã có hiệu lực trong <strong>5 phút</strong>. Vui lòng không chia sẻ mã này.
                    </p>
                    <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                    <p style="font-size: 12px; color: #aaa; text-align: center;">
                        Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.
                    </p>
                </div>
                """
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 15000; // 15s timeout

            try
            {
                _logger.LogInformation("Đang kết nối SMTP tới {Host}:{Port}...", _email.SmtpHost, _email.SmtpPort);
                await client.ConnectAsync(_email.SmtpHost, _email.SmtpPort, SecureSocketOptions.StartTls);
                
                _logger.LogInformation("Xác thực với {User}...", _email.SmtpUser);
                await client.AuthenticateAsync(_email.SmtpUser, _email.SmtpPass);
                
                await client.SendAsync(message);
                _logger.LogInformation("Đã gửi email OTP tới {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email: {Message}", ex.Message);
                throw new InvalidOperationException($"Không thể gửi email: {ex.Message}", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
