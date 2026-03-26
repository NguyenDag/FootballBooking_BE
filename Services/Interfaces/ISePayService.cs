using FootballBooking_BE.Models;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface ISePayService
    {
        Task<List<SePayTransaction>> GetTransactionsAsync(string? accountNumber = null, DateTime? fromDate = null, int limit = 100);
    }
}
