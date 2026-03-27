using FootballBooking_BE.Common;
using FootballBooking_BE.Data;
using FootballBooking_BE.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballBooking_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WalletsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-wallet")]
        public async Task<ActionResult<ApiResponse<WalletDTO>>> GetMyWallet()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy thông tin định danh."));
            
            var userId = int.Parse(userIdStr);

            var wallet = await _context.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new Data.Entities.Wallet { UserId = userId, Balance = 0 };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            var transactions = await _context.Transactions
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .ToListAsync();

            var dto = new WalletDTO
            {
                Balance = wallet.Balance,
                RecentTransactions = transactions.Select(t => new TransactionDTO
                {
                    TransactionId = t.TransactionId,
                    TransactionType = t.TransactionType,
                    Direction = t.Direction,
                    Amount = t.Amount,
                    BalanceBefore = t.BalanceBefore,
                    BalanceAfter = t.BalanceAfter,
                    Description = t.Description,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    BookingId = t.BookingId
                }).ToList()
            };

            return Ok(ApiResponse<WalletDTO>.Ok(dto));
        }

        [HttpPost("pay-booking/{bookingId}")]
        public async Task<ActionResult<ApiResponse<bool>>> PayBooking(int bookingId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized(ApiResponse<object>.Fail("Không tìm thấy thông tin định danh."));
            
            var userId = int.Parse(userIdStr);

            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserId == userId);

            if (booking == null) return NotFound(ApiResponse<bool>.Fail("Không tìm thấy đơn đặt sân."));
            if (booking.PaymentStatus == "PAID") return BadRequest(ApiResponse<bool>.Fail("Đơn hàng đã được thanh toán trước đó."));

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null || wallet.Balance < booking.TotalAmount)
            {
                return BadRequest(ApiResponse<bool>.Fail("Số dư ví không đủ để thanh toán."));
            }

            // Start Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal balanceBefore = wallet.Balance;
                wallet.Balance -= booking.TotalAmount;
                wallet.UpdatedAt = DateTime.UtcNow;

                var paymentTransaction = new Data.Entities.Transaction
                {
                    WalletId = wallet.WalletId,
                    TransactionType = "BOOKING_PAYMENT",
                    Direction = "DEBIT",
                    Amount = booking.TotalAmount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    BookingId = booking.BookingId,
                    Status = "SUCCESS",
                    Description = $"Thanh toán bằng ví cho đơn hàng DH{booking.BookingId}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Transactions.Add(paymentTransaction);

                // Update Booking
                booking.PaymentStatus = "PAID";
                booking.Status = "CONFIRMED";
                foreach (var detail in booking.BookingDetails)
                {
                    if (detail.DetailStatus == "PENDING") detail.DetailStatus = "CONFIRMED";
                }

                // Create Payment record
                var payment = new Data.Entities.Payment
                {
                    BookingId = booking.BookingId,
                    PaymentMethod = "WALLET",
                    Amount = booking.TotalAmount,
                    Status = "SUCCESS",
                    PaidAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Transaction = paymentTransaction
                };
                _context.Payments.Add(payment);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<bool>.Ok(true, "Thanh toán bằng ví thành công!"));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ApiResponse<bool>.Fail("Lỗi hệ thống khi xử lý thanh toán: " + ex.Message));
            }
        }
    }
}
