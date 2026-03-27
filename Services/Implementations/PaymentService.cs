using FootballBooking_BE.Data;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Models;
using FootballBooking_BE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FootballBooking_BE.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;
        private readonly ISePayService _sePayService;

        public PaymentService(AppDbContext context, ILogger<PaymentService> logger, ISePayService sePayService)
        {
            _context = context;
            _logger = logger;
            _sePayService = sePayService;
        }

        public async Task<bool> ProcessWebhookAsync(SePayWebhookPayload payload)
        {
            _logger.LogInformation("Processing SePay Webhook - ID: {Id}, Content: {Content}, Amount: {Amount}", 
                payload.Id, payload.Content, payload.TransferAmount);

            // For v1, we check if TransferAmount > 0 since transfer_type might be missing
            if (payload.TransferAmount <= 0)
            {
                _logger.LogInformation("SePay Webhook: Skipping non-incoming or zero-amount transaction.");
                return false;
            }

            // Regex to find DH{BookingId} or FTB{BookingId} or BK{BookingId} (with optional spaces)
            var match = Regex.Match(payload.Content, @"(?:DH|FTB|BK)\s*(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                _logger.LogWarning("SePay Webhook: Could not find Booking ID in content: '{Content}'. Transaction ID: {Id}", payload.Content, payload.Id);
                return false;
            }

            if (!int.TryParse(match.Groups[1].Value, out int bookingId))
            {
                _logger.LogWarning("SePay Webhook: Failed to parse Booking ID from content.");
                return false;
            }

            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                _logger.LogError("SePay Webhook: Booking with ID {BookingId} not found.", bookingId);
                return false;
            }

            if (booking.PaymentStatus == "PAID")
            {
                _logger.LogInformation("SePay Webhook: Booking {BookingId} is already PAID.", bookingId);
                return true; // Already processed
            }

            // Verify amount
            if (payload.TransferAmount < booking.TotalAmount)
            {
                _logger.LogWarning("SePay Webhook: Amount {TranAmount} is less than Booking amount {BookingAmount} for ID {BookingId}", 
                    payload.TransferAmount, booking.TotalAmount, bookingId);
                return false;
            }

            // Update Booking Status
            booking.PaymentStatus = "PAID";
            booking.Status = "CONFIRMED";

            foreach (var detail in booking.BookingDetails)
            {
                if (detail.DetailStatus == "PENDING")
                {
                    detail.DetailStatus = "CONFIRMED";
                }
            }

            // Create Payment record
            var payment = new Payment
            {
                BookingId = booking.BookingId,
                PaymentMethod = "BANK_TRANSFER",
                Amount = payload.TransferAmount,
                Status = "SUCCESS",
                GatewayTransactionId = payload.Id.ToString(),
                GatewayResponseData = payload.ReferenceCode,
                CreatedAt = DateTime.UtcNow
            };

            if (DateTime.TryParse(payload.TransactionDate, out DateTime paidAt))
            {
                payment.PaidAt = paidAt;
            }
            else 
            {
                payment.PaidAt = DateTime.UtcNow;
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("SePay Webhook: Successfully confirmed payment for Booking ID: {BookingId}", bookingId);
            return true;
        }

        public async Task<int> SyncSePayTransactionsAsync()
        {
            _logger.LogInformation("Starting manual SePay transaction synchronization.");
            var transactions = await _sePayService.GetTransactionsAsync();
            int updatedCount = 0;

            foreach (var tran in transactions)
            {
                // Map SePayTransaction to WebhookPayload
                var payload = new SePayWebhookPayload
                {
                    Id = tran.Id,
                    Content = tran.Content,
                    TransferAmount = tran.AmountIn,
                    TransferType = tran.TransferType,
                    TransactionDate = tran.TransactionDate,
                    ReferenceCode = tran.ReferenceCode
                };

                if (await ProcessWebhookAsync(payload))
                {
                    updatedCount++;
                }
            }

            return updatedCount;
        }
    }
}
