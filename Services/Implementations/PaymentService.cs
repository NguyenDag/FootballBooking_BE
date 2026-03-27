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

        public async Task<PaymentProcessResult> ProcessWebhookAsync(SePayWebhookPayload payload)
        {
            _logger.LogInformation("Processing SePay Webhook - ID: {Id}, Content: {Content}, Amount: {Amount}", 
                payload.Id, payload.Content, payload.TransferAmount);

            // For v1, we check if TransferAmount > 0 since transfer_type might be missing
            if (payload.TransferAmount <= 0)
            {
                _logger.LogInformation("SePay Webhook: Skipping non-incoming or zero-amount transaction.");
                return PaymentProcessResult.Skipped;
            }

            // IDEMPOTENCY CHECK: Check if this SePay transaction was already processed
            var referenceId = payload.Id.ToString();
            bool alreadyExists = await _context.Transactions.AnyAsync(t => t.ReferenceId == referenceId);
            if (alreadyExists)
            {
                _logger.LogInformation("SePay Webhook: Skipping already processed transaction ID: {Id}", payload.Id);
                return PaymentProcessResult.AlreadyProcessed;
            }

            // Regex to find DH{BookingId} or FTB{BookingId} or BK{BookingId} (with optional spaces)
            var match = Regex.Match(payload.Content, @"(?:DH|FTB|BK)\s*(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                _logger.LogWarning("SePay Webhook: Could not find Booking ID in content: '{Content}'. Transaction ID: {Id}", payload.Content, payload.Id);
                return PaymentProcessResult.Skipped;
            }

            if (!int.TryParse(match.Groups[1].Value, out int bookingId))
            {
                _logger.LogWarning("SePay Webhook: Failed to parse Booking ID from content.");
                return PaymentProcessResult.Skipped;
            }

            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                _logger.LogError("SePay Webhook: Booking with ID {BookingId} not found.", bookingId);
                return PaymentProcessResult.Error;
            }

            if (booking.PaymentStatus == "PAID")
            {
                _logger.LogInformation("SePay Webhook: Booking {BookingId} is already PAID.", bookingId);
                return PaymentProcessResult.AlreadyPaid;
            }

            // 1. Find or Create Wallet for User
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == booking.UserId);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = booking.UserId, Balance = 0 };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            // 2. Perform Top-up (CREDIT)
            decimal balanceBeforeTopUp = wallet.Balance;
            wallet.Balance += payload.TransferAmount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var topUpTransaction = new Transaction
            {
                WalletId = wallet.WalletId,
                TransactionType = "TOP_UP",
                Direction = "CREDIT",
                Amount = payload.TransferAmount,
                BalanceBefore = balanceBeforeTopUp,
                BalanceAfter = wallet.Balance,
                Status = "SUCCESS",
                ReferenceId = payload.Id.ToString(),
                Description = $"SePay Top-up: {payload.Content}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Transactions.Add(topUpTransaction);

            _logger.LogInformation("SePay: Topped up wallet for User {UserId}. New Balance: {Balance}", booking.UserId, wallet.Balance);

            PaymentProcessResult finalResult = PaymentProcessResult.TopUpOnly;

            // 3. Perform Payment if possible and if not already paid
            if (booking.PaymentStatus != "PAID" && wallet.Balance >= booking.TotalAmount)
            {
                decimal balanceBeforePayment = wallet.Balance;
                wallet.Balance -= booking.TotalAmount;
                wallet.UpdatedAt = DateTime.UtcNow;

                var paymentTransaction = new Transaction
                {
                    WalletId = wallet.WalletId,
                    TransactionType = "BOOKING_PAYMENT",
                    Direction = "DEBIT",
                    Amount = booking.TotalAmount,
                    BalanceBefore = balanceBeforePayment,
                    BalanceAfter = wallet.Balance,
                    BookingId = booking.BookingId,
                    Status = "SUCCESS",
                    Description = $"Payment for Booking DH{booking.BookingId}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Transactions.Add(paymentTransaction);

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

                // Create Payment record linked to transaction
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentMethod = "BANK_TRANSFER",
                    Amount = booking.TotalAmount,
                    Status = "SUCCESS",
                    GatewayTransactionId = payload.Id.ToString(),
                    GatewayResponseData = payload.ReferenceCode,
                    Transaction = paymentTransaction,
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
                _logger.LogInformation("SePay: Processed payment for Booking {BookingId} using wallet.", booking.BookingId);
                finalResult = PaymentProcessResult.BookingPaid;
            }

            await _context.SaveChangesAsync();
            return finalResult;
        }

        public async Task<SyncResult> SyncSePayTransactionsAsync()
        {
            _logger.LogInformation("Starting manual SePay transaction synchronization.");
            var transactions = await _sePayService.GetTransactionsAsync();
            var syncResult = new SyncResult();

            foreach (var tran in transactions)
            {
                // Regex to find BookingId in content for tracking
                var match = Regex.Match(tran.Content, @"(?:DH|FTB|BK)\s*(\d+)", RegexOptions.IgnoreCase);
                int? bookingId = null;
                if (match.Success && int.TryParse(match.Groups[1].Value, out int bId))
                {
                    bookingId = bId;
                }

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

                var processStatus = await ProcessWebhookAsync(payload);
                if (processStatus != PaymentProcessResult.Skipped && processStatus != PaymentProcessResult.AlreadyProcessed)
                {
                    syncResult.TotalProcessed++;
                    if (bookingId.HasValue)
                    {
                        if (processStatus == PaymentProcessResult.BookingPaid)
                        {
                            syncResult.PaidBookingIds.Add(bookingId.Value);
                        }
                        else if (processStatus == PaymentProcessResult.TopUpOnly)
                        {
                            syncResult.PartiallyPaidBookingIds.Add(bookingId.Value);
                        }
                    }
                }
            }

            return syncResult;
        }
    }
}
