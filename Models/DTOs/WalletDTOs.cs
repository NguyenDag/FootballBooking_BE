using System;
using System.Collections.Generic;

namespace FootballBooking_BE.Models.DTOs
{
    public class WalletDTO
    {
        public decimal Balance { get; set; }
        public List<TransactionDTO> RecentTransactions { get; set; } = new();
    }

    public class TransactionDTO
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; } = null!;
        public string Direction { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int? BookingId { get; set; }
    }
}
