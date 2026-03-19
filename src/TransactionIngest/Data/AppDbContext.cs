using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TransactionIngest.Data.Entities;

namespace TransactionIngest.Data;

public class AppDbContext : DbContext
{
    public DbSet<TransactionRecord> Transactions => Set<TransactionRecord>();
    public DbSet<TransactionAudit> Audits => Set<TransactionAudit>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        model.Entity<TransactionRecord>()
            .HasIndex(t => t.TransactionId)
            .IsUnique();
    }

    public async Task<int> SaveChangesIgnoringUniqueViolationsAsync()
    {
        while (true)
        {
            try
            {
                return await base.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                var entry = ex.Entries.Single();

                // Remove the offending entity from the change tracker
                entry.State = EntityState.Detached;

                // Loop continues and SaveChangesAsync() is retried
            }
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqliteException sqliteEx)
            return sqliteEx.SqliteErrorCode == 19; // SQLITE_CONSTRAINT

        return false;
    }
}