using TransactionIngest.Ingestion;
using Microsoft.Extensions.Logging.Abstractions;
using TransactionIngest.Models;
using TransactionIngest.Data.Entities;
using Xunit;
using TransactionIngest.Test.TestHelpers;

namespace TransactionIngest.Tests;

public class IngestionTests
{
    [Fact]
    public async Task Inserts_New_Transactions()
    {
        var db = TestDb.Create();
        var logger = NullLogger<SnapshotProcessor>.Instance;
        var processor = new SnapshotProcessor(db, logger);

        var feed = new List<TransactionSnapshotDto>
        {
            new()
            {
                TransactionId = 1001,
                CardNumber = "4111111111111111",
                LocationCode = "STO-01",
                ProductName = "Mouse",
                Amount = 19.99m,
                Timestamp = DateTime.UtcNow
            }
        };

        await processor.ProcessAsync(feed);

        var record = db.Transactions.Single();
        Assert.Equal(1001, record.TransactionId);
        Assert.Equal("1111", record.CardLast4);
        Assert.Equal(TransactionStatus.Active, record.Status);

        Assert.Single(db.Audits);
        Assert.Equal("Insert", db.Audits.Single().ChangeType);
    }
}