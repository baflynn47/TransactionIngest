using TransactionIngest.Ingestion;
using Microsoft.Extensions.Logging.Abstractions;
using TransactionIngest.Models;
using TransactionIngest.Test.TestHelpers;
using Xunit;

namespace TransactionIngest.Tests;

public class UpdateDetectionTests
{
    [Fact]
    public async Task Detects_Field_Changes()
    {
        var db = TestDb.Create();
        var logger = NullLogger<SnapshotProcessor>.Instance;
        var processor = new SnapshotProcessor(db, logger);

        var initial = new List<TransactionSnapshotDto>
        {
            new()
            {
                TransactionId = 2001,
                CardNumber = "4111111111111111",
                LocationCode = "STO-01",
                ProductName = "Mouse",
                Amount = 10m,
                Timestamp = DateTime.UtcNow
            }
        };

        await processor.ProcessAsync(initial);

        var updated = new List<TransactionSnapshotDto>
        {
            new()
            {
                TransactionId = 2001,
                CardNumber = "4111111111111111",
                LocationCode = "STO-02", // changed
                ProductName = "Mouse",
                Amount = 12m, // changed
                Timestamp = DateTime.UtcNow
            }
        };

        await processor.ProcessAsync(updated);

        var audits = db.Audits.Where(a => a.ChangeType == "Update").ToList();
        Assert.Single(audits);

        var details = audits.Single().Details;
        Assert.Contains("LocationCode", details);
        Assert.Contains("Amount", details);
    }
}