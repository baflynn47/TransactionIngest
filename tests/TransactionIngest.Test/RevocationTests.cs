using TransactionIngest.Data.Entities;
using TransactionIngest.Ingestion;
using Microsoft.Extensions.Logging.Abstractions;
using TransactionIngest.Models;
using TransactionIngest.Test.TestHelpers;
using Xunit;

namespace TransactionIngest.Tests;

public class RevocationTests
{
    [Fact]
    public async Task Marks_Missing_Transactions_As_Revoked()
    {
        var db = TestDb.Create();
        var logger = NullLogger<SnapshotProcessor>.Instance;
        var processor = new SnapshotProcessor(db, logger);

        var initial = new List<TransactionSnapshotDto>
        {
            new()
            {
                TransactionId = 3001,
                CardNumber = "4111111111111111",
                LocationCode = "STO-01",
                ProductName = "Mouse",
                Amount = 10m,
                Timestamp = DateTime.UtcNow
            }
        };

        await processor.ProcessAsync(initial);

        // Now feed is empty → record should be revoked
        await processor.ProcessAsync(new List<TransactionSnapshotDto>());

        var record = db.Transactions.Single();
        Assert.Equal(TransactionStatus.Revoked, record.Status);

        var audit = db.Audits.Single(a => a.ChangeType == "Revoke");
        Assert.Contains("3001", audit.Details);
    }
}