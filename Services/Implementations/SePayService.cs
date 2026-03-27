using FootballBooking_BE.Models;
using FootballBooking_BE.Services.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace FootballBooking_BE.Services.Implementations
{
    public class SePayService : ISePayService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<SePaySettings> _settings;
        private readonly ILogger<SePayService> _logger;

        public SePayService(HttpClient httpClient, IOptions<SePaySettings> settings, ILogger<SePayService> logger)
        {
            _httpClient = httpClient;
            _settings = settings;
            _logger = logger;
        }

        public async Task<List<SePayTransaction>> GetTransactionsAsync(string? accountNumber = null, DateTime? fromDate = null, int limit = 100)
        {
            try
            {
                // SePay v1 logic (as requested by user "Hướng 2")
                // URL: https://my.sepay.vn/userapi/transactions/list
                // Auth: Authorization: Bearer <API_TOKEN> (in header)
                var url = "https://my.sepay.vn/userapi/transactions/list?limit=" + limit;
                if (!string.IsNullOrEmpty(accountNumber)) url += "&account_number=" + accountNumber;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.Value.ApiToken}");

                _logger.LogInformation("Syncing SePay v1: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                var rawJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("SePay API Error: {Status} - {Error}", response.StatusCode, rawJson);
                    return new List<SePayTransaction>();
                }

                // Log the first 200 chars to see the structure
                _logger.LogInformation("SePay Raw Response: {Raw}", rawJson.Length > 200 ? rawJson.Substring(0, 200) : rawJson);
                try { System.IO.File.AppendAllText("C:\\Windows\\Temp\\sepay_sync.log", $"[{DateTime.Now}] Sync Response: {rawJson}\n"); } catch {}

                var data = System.Text.Json.JsonSerializer.Deserialize<SePayTransactionResponse>(rawJson);
                var list = data?.Transactions ?? new List<SePayTransaction>();
                _logger.LogInformation("Fetched {Count} transactions from SePay.", list.Count);
                
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching SePay transactions.");
                return new List<SePayTransaction>();
            }
        }
    }
}
