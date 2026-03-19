using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace TransactionIngest.Data.Entities;

public class TransactionRecord
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    [MaxLength(19)] 
    public string CardLast4 { get; set; } = "";
    [MaxLength(19)]
    public string LocationCode { get; set; } = "";
    [MaxLength(19)]
    public string ProductName { get; set; } = "";
    [Precision(18, 2)]
    public decimal Amount { get; set; }
    public DateTime TransactionTime { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Active;

    public List<TransactionAudit> Audits { get; set; } = new();
}