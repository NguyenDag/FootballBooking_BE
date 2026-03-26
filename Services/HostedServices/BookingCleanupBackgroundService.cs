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

                // Chạy mỗi 5 phút
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("BookingCleanupBackgroundService is stopping.");
        }

        private async Task ProcessCleanupAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var thirtyMinsFromNow = DateTime.Now.AddMinutes(30);

            // Tìm các BookingDetail PENDING và sắp đến giờ đá (< 30 phút) hoặc đã qua giờ đá
            var overdueDetails = await dbContext.BookingDetails
                .Include(d => d.Booking)
                .Where(d => d.DetailStatus == BookingStatus.Pending)
                .ToListAsync();

            var detailsToCancel = overdueDetails.Where(d =>
            {
                var playDateTime = d.PlayDate.ToDateTime(new TimeOnly(d.StartTime.Ticks));
                return playDateTime <= thirtyMinsFromNow;
            }).ToList();

            if (!detailsToCancel.Any()) return;

            _logger.LogInformation($"Found {detailsToCancel.Count} pending bookings to auto-cancel.");

            var changedBookings = new HashSet<Booking>();

            foreach (var detail in detailsToCancel)
            {
                string oldStatus = detail.DetailStatus;
                detail.DetailStatus = BookingStatus.Cancelled;
                detail.CancellationReason = "Hệ thống tự động hủy do quá thời hạn xác nhận";

                dbContext.BookingStatusHistories.Add(new BookingStatusHistory
                {
                    BookingDetailId = detail.DetailId,
                    OldStatus = oldStatus,
                    NewStatus = BookingStatus.Cancelled,
                    ChangedBy = null, // System Action
                    ChangedAt = DateTime.UtcNow
                });

                changedBookings.Add(detail.Booking);
            }

            // Xử lý hoàn tiền cho Booking cha và đồng bộ trạng thái
            foreach (var booking in changedBookings)
            {
                // Kiểm tra xem tất cả các details trong booking list đã bị CANCELLED/REJECTED chưa
                var allDbDetails = await dbContext.BookingDetails
                    .Where(d => d.BookingId == booking.BookingId)
                    .ToListAsync();
                
                if (allDbDetails.All(d => 
                    d.DetailStatus.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase) || 
                    d.DetailStatus.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase)))
                {
                    booking.Status = BookingStatus.Cancelled;

                    // Nếu đã thanh toán, tiến hành hoàn 100% tiền vào Ví
                    if (booking.PaymentStatus == "PAID")
                    {
                        var wallet = await dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == booking.UserId);
                        if (wallet != null)
                        {
                            var oldBalance = wallet.Balance;
                            wallet.Balance += booking.TotalAmount;
                            
                            booking.PaymentStatus = "REFUNDED";

                            dbContext.Transactions.Add(new Transaction
                            {
                                WalletId = wallet.WalletId,
                                TransactionType = "REFUND",
                                Direction = "CREDIT",
                                Amount = booking.TotalAmount,
                                BalanceBefore = oldBalance,
                                BalanceAfter = wallet.Balance,
                                BookingId = booking.BookingId,
                                ReferenceId = $"AUTO_REFUND_{booking.BookingId}",
                                Description = "Hoàn tiền tự động do hệ thống hủy lịch treo",
                                Status = "SUCCESS",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Auto-cancellation and refund processing completed.");
        }
    }
}
