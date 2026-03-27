using FootballBooking_BE.Common;
using FootballBooking_BE.Data;
using FootballBooking_BE.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballBooking_BE.Services.HostedServices
{
    public class BookingCleanupBackgroundService : BackgroundService
    {
        private readonly ILogger<BookingCleanupBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BookingCleanupBackgroundService(ILogger<BookingCleanupBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingCleanupBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing BookingCleanupBackgroundService.");
                }

                // Chạy mỗi 1 phút để kiểm tra chính xác các đơn quá hạn 10 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("BookingCleanupBackgroundService is stopping.");
        }

        private async Task ProcessCleanupAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var vnNow = DateTime.UtcNow.AddHours(7);
            var tenMinsAgo = DateTime.UtcNow.AddMinutes(-10); // CreatedAt is Utc
            var thirtyMinsFromNowVn = vnNow.AddMinutes(30);

            // 1. Tự động hủy (CANCEL) các đơn PENDING nhưng đã sát giờ đá (< 30p)
            var pendingDetails = await dbContext.BookingDetails
                .Include(d => d.Booking)
                .Where(d => d.DetailStatus == BookingStatus.Pending)
                .ToListAsync();

            var detailsToCancel = pendingDetails.Where(d =>
            {
                var playDateTime = d.PlayDate.ToDateTime(new TimeOnly(d.StartTime.Ticks));
                return playDateTime <= thirtyMinsFromNowVn;
            }).ToList();

            // 2. Tự động hủy (CANCEL) các đơn PENDING + UNPAID quá 10 phút
            var expiredUnpaidBookings = await dbContext.Bookings
                .Include(b => b.BookingDetails)
                .Where(b => b.Status == BookingStatus.Pending && 
                            b.PaymentStatus == PaymentStatus.Unpaid && 
                            b.CreatedAt <= tenMinsAgo)
                .ToListAsync();

            foreach (var booking in expiredUnpaidBookings)
            {
                foreach (var detail in booking.BookingDetails)
                {
                    if (detail.DetailStatus == BookingStatus.Pending && !detailsToCancel.Any(d => d.DetailId == detail.DetailId))
                    {
                        detail.CancellationReason = "Hệ thống tự động hủy do không thanh toán trong vòng 10 phút";
                        detailsToCancel.Add(detail);
                    }
                }
            }

            // Thực hiện hủy và hoàn tiền (nếu có)
            if (detailsToCancel.Any())
            {
                _logger.LogInformation($"Auto-cancelling {detailsToCancel.Count} booking details.");
                var changedBookings = new HashSet<Booking>();

                foreach (var detail in detailsToCancel)
                {
                    string oldStatus = detail.DetailStatus;
                    detail.DetailStatus = BookingStatus.Cancelled;
                    if (string.IsNullOrEmpty(detail.CancellationReason))
                        detail.CancellationReason = "Hệ thống tự động hủy do quá thời gian xác nhận";

                    dbContext.BookingStatusHistories.Add(new BookingStatusHistory
                    {
                        BookingDetailId = detail.DetailId,
                        OldStatus = oldStatus,
                        NewStatus = BookingStatus.Cancelled,
                        ChangedAt = DateTime.UtcNow
                    });
                    changedBookings.Add(detail.Booking);
                }

                foreach (var booking in changedBookings)
                {
                    var allDbDetails = await dbContext.BookingDetails.Where(d => d.BookingId == booking.BookingId).ToListAsync();
                    if (allDbDetails.All(d => d.DetailStatus == BookingStatus.Cancelled || d.DetailStatus == BookingStatus.Rejected))
                    {
                        booking.Status = BookingStatus.Cancelled;
                        if (booking.PaymentStatus == PaymentStatus.Paid)
                        {
                            var wallet = await dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == booking.UserId);
                            if (wallet != null)
                            {
                                var oldBalance = wallet.Balance;
                                wallet.Balance += booking.TotalAmount;
                                booking.PaymentStatus = PaymentStatus.Refunded;
                                dbContext.Transactions.Add(new Transaction
                                {
                                    WalletId = wallet.WalletId,
                                    TransactionType = "REFUND", Direction = "CREDIT", Amount = booking.TotalAmount,
                                    BalanceBefore = oldBalance, BalanceAfter = wallet.Balance,
                                    BookingId = booking.BookingId, ReferenceId = $"AUTO_REFUND_{booking.BookingId}",
                                    Description = "Hoàn tiền tự động do hệ thống hủy lịch treo",
                                    Status = "SUCCESS", CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
            }

            var confirmedDetails = await dbContext.BookingDetails
                .Include(d => d.Booking)
                .Where(d => d.DetailStatus == BookingStatus.Confirmed && d.Booking.PaymentStatus == PaymentStatus.Paid)
                .ToListAsync();

            var detailsToComplete = confirmedDetails.Where(d =>
            {
                var playEndDateTime = d.PlayDate.ToDateTime(new TimeOnly(d.EndTime.Ticks));
                return playEndDateTime <= vnNow;
            }).ToList();

            if (detailsToComplete.Any())
            {
                _logger.LogInformation($"Auto-completing {detailsToComplete.Count} finished booking details.");
                var completedBookings = new HashSet<Booking>();

                foreach (var detail in detailsToComplete)
                {
                    string oldStatus = detail.DetailStatus;
                    detail.DetailStatus = BookingStatus.Completed;
                    dbContext.BookingStatusHistories.Add(new BookingStatusHistory
                    {
                        BookingDetailId = detail.DetailId,
                        OldStatus = oldStatus,
                        NewStatus = BookingStatus.Completed,
                        ChangedAt = DateTime.UtcNow
                    });
                    completedBookings.Add(detail.Booking);
                }

                foreach (var booking in completedBookings)
                {
                    var allDetails = await dbContext.BookingDetails.Where(d => d.BookingId == booking.BookingId).ToListAsync();
                    if (allDetails.All(d => d.DetailStatus == BookingStatus.Completed))
                    {
                        booking.Status = BookingStatus.Completed;
                    }
                }
            }

            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Background task: Changes saved successfully.");
            }
        }
    }
}
