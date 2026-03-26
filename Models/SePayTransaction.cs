using System.Text.Json.Serialization;

namespace FootballBooking_BE.Models
{
    public class SePayTransactionResponse
    {
        [JsonPropertyName("data")]
        public List<SePayTransaction> Transactions { get; set; } = new();
    }

    public class SePayTransaction
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("transactionDate")]
        public string TransactionDate { get; set; } = string.Empty;

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("transferType")]
        public string TransferType { get; set; } = string.Empty;

        [JsonPropertyName("transferAmount")]
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

        [JsonPropertyName("referenceCode")]
        public string ReferenceCode { get; set; } = string.Empty;
    }
}
