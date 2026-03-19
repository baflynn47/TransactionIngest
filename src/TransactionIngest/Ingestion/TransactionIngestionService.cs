using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TransactionIngest.Data;
using TransactionIngest.Ingestion;

namespace TransactionIngest.Ingestion;

public class TransactionIngestionService
{
    private readonly AppDbContext _db;
    private readonly ITransactionFeedClient _client;
    private readonly SnapshotProcessor _processor;
    private readonly ILogger<TransactionIngestionService> _logger;

    public TransactionIngestionService(AppDbContext db,
        ITransactionFeedClient client,
        SnapshotProcessor processor,
        ILogger<TransactionIngestionService> logger)
    {
        _db = db;
        _client = client;
        _processor = processor;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        var snapshot = await _client.FetchSnapshotAsync();
        _logger.LogInformation("Fetched {Count} transactions from feed", snapshot.Count);

        await _processor.ProcessAsync(snapshot);

        await tx.CommitAsync();

        _logger.LogInformation("Ingestion run completed successfully at {Time}", DateTime.UtcNow);
    }
}