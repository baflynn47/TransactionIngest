using System.Text.Json.Serialization;

namespace TransactionIngest.Models;

public class TransactionSnapshotDto
{
    [JsonPropertyName("transactionId")]
    public int TransactionId { get; set; }
    [JsonPropertyName("cardNumber")]
    public string CardNumber { get; set; } = "";
    [JsonPropertyName("locationCode")]
    public string LocationCode { get; set; } = "";
    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = "";
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}