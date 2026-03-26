using FootballBooking_BE.Models;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> ProcessWebhookAsync(SePayWebhookPayload payload);
        Task<int> SyncSePayTransactionsAsync();
    }
}
