using FootballBooking_BE.Models;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentProcessResult> ProcessWebhookAsync(SePayWebhookPayload payload);
        Task<SyncResult> SyncSePayTransactionsAsync();
    }
}
