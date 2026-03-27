using System.Text.Json.Serialization;

namespace FootballBooking_BE.Models
{
    public class SePayTransactionResponse
    {
        [JsonPropertyName("transactions")]
        public List<SePayTransaction> Transactions { get; set; } = new();
    }

    public class SePayTransaction
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("transaction_date")]
        public string TransactionDate { get; set; } = string.Empty;

        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; } = string.Empty;

        [JsonPropertyName("transaction_content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("transfer_type")]
        public string TransferType { get; set; } = string.Empty;

        [JsonPropertyName("amount_in")]
        public System.Text.Json.JsonElement AmountInElement { get; set; }

        public decimal AmountIn => GetDecimal(AmountInElement);

        private decimal GetDecimal(System.Text.Json.JsonElement element)
        {
            if (element.ValueKind == System.Text.Json.JsonValueKind.Number)
                return element.GetDecimal();
            if (element.ValueKind == System.Text.Json.JsonValueKind.String && decimal.TryParse(element.GetString(), out var val))
                return val;
            return 0;
        }

        [JsonPropertyName("reference_number")]
        public string ReferenceCode { get; set; } = string.Empty;
    }
}
