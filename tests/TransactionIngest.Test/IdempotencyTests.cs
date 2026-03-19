using TransactionIngest.Ingestion;
using Microsoft.Extensions.Logging.Abstractions;
using TransactionIngest.Models;
using TransactionIngest.Test.TestHelpers;
using Xunit;

namespace TransactionIngest.Tests;

public class IdempotencyTests
{
    [Fact]
    public async Task Repeated_Runs_Do_Not_Create_Duplicates()
    {
        var db = TestDb.Create();
        var logger = NullLogger<SnapshotProcessor>.Instance;
        var processor = new SnapshotProcessor(db, logger);

        var feed = new List<TransactionSnapshotDto>
        {
            new()
            {
                TransactionId = 4001,
                CardNumber = "4111111111111111",
                LocationCode = "STO-01",
                ProductName = "Mouse",
                Amount = 10m,
                Timestamp = DateTime.UtcNow
            }
        };

        await processor.ProcessAsync(feed);
        await processor.ProcessAsync(feed); // run twice

        Assert.Single(db.Transactions);
        Assert.Single(db.Audits.Where(a => a.ChangeType == "Insert"));
        Assert.Empty(db.Audits.Where(a => a.ChangeType == "Update"));
    }
}