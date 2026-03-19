using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using TransactionIngest.Data;
using TransactionIngest.Ingestion;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddJsonFile("appsettings.Development.json", optional: true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(context.Configuration.GetConnectionString("Default")));

        services.AddSingleton<ITransactionFeedClient, MockTransactionFeedClient>();
        services.AddScoped<TransactionIngestionService>();
        services.AddScoped<SnapshotProcessor>();
    })
    .Build();

// entity framework migrations should be applied at startup to ensure the database is ready before processing transactions
using var migrateScope = host.Services.CreateScope();
var db = migrateScope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.MigrateAsync();   // Creates DB + applies migrations

// run the ingestion service to start processing transactions
using var ingestionScope = host.Services.CreateScope();
var svc = ingestionScope.ServiceProvider.GetRequiredService<TransactionIngestionService>();
await svc.RunAsync();