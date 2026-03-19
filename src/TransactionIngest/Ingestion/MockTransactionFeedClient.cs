using Microsoft.Extensions.Configuration;
using System.Text.Json;
using TransactionIngest.Ingestion;
using TransactionIngest.Models;

namespace TransactionIngest.Ingestion;

public class MockTransactionFeedClient : ITransactionFeedClient
{
    private readonly string _path;

    public MockTransactionFeedClient(IConfiguration config)
    {
        _path = config["Feed:MockJsonPath"] ?? "mock-feed.json";
    }

    public async Task<List<TransactionSnapshotDto>> FetchSnapshotAsync()
    {
        if (!File.Exists(_path))
            return new List<TransactionSnapshotDto>();

        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<List<TransactionSnapshotDto>>(json)
               ?? new List<TransactionSnapshotDto>();
    }
}