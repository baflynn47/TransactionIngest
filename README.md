📘 TransactionsIngest
A lightweight, testable ingestion pipeline for processing transaction snapshots, detecting changes, recording audits, and maintaining a clean historical record.
This project is built with:
- .NET 10
- EF Core 8 (SQLite)
- xUnit
- Configuration + Hosting
- Dependency Injection
- Mock JSON feed for local testing

🚀 Overview
The ingestion job processes a snapshot of transactions from an external feed.
Each run:
- Loads the latest snapshot
- Upserts new and updated transactions
- Detects field‑level changes
- Writes audit records
- Marks missing transactions as Revoked
- Marks transactions older than 24 hours as Finalized
- Ensures idempotency (safe to run repeatedly)
The system is designed to be:
- Deterministic
- Auditable
- Easy to test
- Easy to extend


⚙️ How It Works
🧩 SnapshotProcessor
The core ingestion logic lives in:
src/TransactionsIngest/Ingestion/SnapshotProcessor.cs


🗄️ Database
The app uses SQLite for simplicity.
Connection string:
"ConnectionStrings": {
  "Default": "Data Source=transactions.db"
}


Automatic Migrations
Program.cs applies migrations on startup:
using var migrateScope = host.Services.CreateScope();
var db = migrateScope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.MigrateAsync();   // Creates DB + applies migrations


🧪 Running Tests
The test suite uses:
- xUnit
- EF Core InMemory
- Isolated DbContext per test
Run all tests:
dotnet test


Test coverage includes:
- Inserts
- Updates
- Revocations
- Idempotency
- Finalization

🧪 Mock Feed
A realistic mock feed is included:
mock-feed.json


You can modify this file to simulate:
- Missing transactions
- Updated fields
- Late arrivals
- Out‑of‑order timestamps

▶️ Running the App
From the project root:
cd src/TransactionsIngest
dotnet run


The ingestion job will:
- Apply migrations
- Load the mock feed
- Process the snapshot
- Write results to transactions.db


