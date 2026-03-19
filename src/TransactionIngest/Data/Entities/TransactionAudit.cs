using TransactionIngest.Data.Entities;

namespace TransactionIngest.Data.Entities;

public class TransactionAudit
{
    public int Id { get; set; }
    public int TransactionRecordId { get; set; }
    public TransactionRecord Record { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string ChangeType { get; set; } = "";
    public string Details { get; set; } = "";
}