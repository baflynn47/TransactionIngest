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

        //model.Entity<TransactionAudit>()
        //    .HasOne(ta => ta.Record)
        //    .WithMany(r => r.Audits)
        //    .HasForeignKey(ta => ta.Id);

        //model.Entity<TransactionRecord>()
        //    .HasIndex(t => t.TransactionId)
        //    .IsUnique();
    }
}