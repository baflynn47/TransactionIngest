using TransactionIngest.Data.Entities;
using TransactionIngest.Ingestion;
using Microsoft.Extensions.Logging.Abstractions;
using TransactionIngest.Models;
using TransactionIngest.Test.TestHelpers;
using Xunit;

namespace TransactionIngest.Tests;

public class FinalizationTests
{
    [Fact]
    public async Task Finalizes_Transactions_Older_Than_24_Hours()
    {
        var db = TestDb.Create();
        var logger = NullLogger<SnapshotProcessor>.Instance;
        var processor = new SnapshotProcessor(db, logger);

        var oldTimestamp = DateTime.UtcNow.AddHours(-30);

        var feed = new List<TransactionSnapshotDto>
        {
            new()
            {
                TransactionId = 5001,
                CardNumber = "4111111111111111",
                LocationCode = "STO-01",
                ProductName = "Mouse",
                Amount = 10m,
                Timestamp = oldTimestamp
            }
        };

        // insert record into the database
        await processor.ProcessAsync(feed);

        // Second run triggers finalization; record isn't in feed because it's old
        await processor.ProcessAsync(new List<TransactionSnapshotDto>());

        var record = db.Transactions.Single();
        Assert.Equal(TransactionStatus.Finalized, record.Status);

        Assert.Contains(db.Audits, a => a.ChangeType == "Finalize");
    }
}