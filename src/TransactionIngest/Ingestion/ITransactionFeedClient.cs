using TransactionIngest.Models;

namespace TransactionIngest.Ingestion;

public interface ITransactionFeedClient
{
    Task<List<TransactionSnapshotDto>> FetchSnapshotAsync();
}
