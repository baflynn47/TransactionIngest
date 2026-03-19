using Microsoft.EntityFrameworkCore;
using TransactionIngest.Data;

namespace TransactionIngest.Test.TestHelpers;

public static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
