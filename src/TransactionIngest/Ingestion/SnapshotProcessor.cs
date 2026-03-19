using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionIngest.Data;
using TransactionIngest.Data.Entities;
using TransactionIngest.Models;
using TransactionIngest.Utils;

namespace TransactionIngest.Ingestion;

public class SnapshotProcessor
{
    private readonly AppDbContext _db;
    private readonly ILogger<SnapshotProcessor> _logger;

    public SnapshotProcessor(AppDbContext db, ILogger<SnapshotProcessor> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ProcessAsync(List<TransactionSnapshotDto> snapshot)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddHours(-24);
        
        // Load all existing records within the last 24 hours
        var existing = await _db.Transactions
            .Where(t => t.TransactionTime >= cutoff)
            .ToListAsync();

        var existingById = existing.ToDictionary(t => t.TransactionId);

        // Track which IDs we saw in the snapshot
        var seenIds = new HashSet<int>();

        foreach (var dto in snapshot)
        {
            seenIds.Add(dto.TransactionId);

            if (!existingById.TryGetValue(dto.TransactionId, out var record))
            {
                _logger.LogInformation("Inserting new transaction {Id}", dto.TransactionId);

                // INSERT
                record = new TransactionRecord
                {
                    TransactionId = dto.TransactionId,
                    CardLast4 = Hashing.Last4(dto.CardNumber),
                    LocationCode = dto.LocationCode,
                    ProductName = dto.ProductName,
                    Amount = dto.Amount,
                    TransactionTime = dto.Timestamp,
                    Status = TransactionStatus.Active
                };

                _db.Transactions.Add(record);

                _db.Audits.Add(new TransactionAudit
                {
                    Record = record,
                    Timestamp = now,
                    ChangeType = "Insert",
                    Details = $"Inserted transaction {dto.TransactionId}"
                });

                continue;
            }

            // already finalized, ignore any changes but log if it reappears in snapshot
            if (record.Status == TransactionStatus.Finalized)
            {
                _logger.LogWarning("Transaction {Id} was finalized but reappeared in snapshot, ignoring any changes", record.TransactionId);
                continue;
            }

            // UPDATE detection
            var changes = new List<string>();

            if (record.CardLast4 != Hashing.Last4(dto.CardNumber))
            {
                changes.Add($"CardLast4: '{record.CardLast4}' → '{Hashing.Last4(dto.CardNumber)}'");
                record.CardLast4 = Hashing.Last4(dto.CardNumber);
            }

            if (record.LocationCode != dto.LocationCode)
            {
                changes.Add($"LocationCode: '{record.LocationCode}' → '{dto.LocationCode}'");
                record.LocationCode = dto.LocationCode;
            }

            if (record.ProductName != dto.ProductName)
            {
                changes.Add($"ProductName: '{record.ProductName}' → '{dto.ProductName}'");
                record.ProductName = dto.ProductName;
            }

            if (record.Amount != dto.Amount)
            {
                changes.Add($"Amount: {record.Amount} → {dto.Amount}");
                record.Amount = dto.Amount;
            }

            if (record.TransactionTime != dto.Timestamp)
            {
                changes.Add($"TransactionTime: {record.TransactionTime:o} → {dto.Timestamp:o}");
                record.TransactionTime = dto.Timestamp;
            }

            if (changes.Count > 0)
            {
                _logger.LogInformation("Updating transaction {Id}: {Changes}", dto.TransactionId, string.Join("; ", changes));

                _db.Audits.Add(new TransactionAudit
                {
                    Record = record,
                    Timestamp = now,
                    ChangeType = "Update",
                    Details = string.Join("; ", changes)
                });
            }

            // Ensure status is active if present in snapshot
            if (record.Status == TransactionStatus.Revoked)
            {
                _logger.LogWarning("Updating transaction {Id}: Switching Revoked to Activate", record.TransactionId);


                record.Status = TransactionStatus.Active;

                _db.Audits.Add(new TransactionAudit
                {
                    Record = record,
                    Timestamp = now,
                    ChangeType = "StatusChange",
                    Details = "Revoked → Active (reappeared in snapshot)"
                });
            }
        }

        // REVOCATION: any existing record not seen in snapshot
        foreach (var record in existing)
        {
            if (!seenIds.Contains(record.TransactionId) &&
                record.TransactionTime >= cutoff &&
                record.Status == TransactionStatus.Active)
            {
                _logger.LogWarning("Updating transaction {Id}: Switching Revoked to Activate", record.TransactionId);

                record.Status = TransactionStatus.Revoked;

                _db.Audits.Add(new TransactionAudit
                {
                    Record = record,
                    Timestamp = now,
                    ChangeType = "Revoke",
                    Details = $"Transaction {record.TransactionId} revoked (missing from snapshot)"
                });
            }
        }

        // records returned do not include new records or records updated to finalized
        var oldRecords = await _db.Transactions
            // Exclude Finalized && Revoked records
            .Where(t => t.TransactionTime < cutoff && t.Status == TransactionStatus.Active)
            .ToListAsync();

        foreach (var record in oldRecords)
        {
            _logger.LogInformation("Finalizing transaction {Id}", record.TransactionId);

            record.Status = TransactionStatus.Finalized;

            _db.Audits.Add(new TransactionAudit
            {
                Record = record,
                Timestamp = now,
                ChangeType = "Finalize",
                Details = $"Transaction {record.TransactionId} finalized (>24h old)"
            });
        }

        await _db.SaveChangesIgnoringUniqueViolationsAsync();
    }
}