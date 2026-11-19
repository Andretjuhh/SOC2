using CoreApi.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreApi.Tests;

[CollectionDefinition("Shared database", DisableParallelization = true)]
public sealed class SharedDatabaseCollection : ICollectionFixture<TicketTestDatabase>
{
}

// Creates an isolated SQL Server test database for this test collection.
// A relational database is required for the history trigger and concurrency behavior.
public sealed class TicketTestDatabase : IDisposable
{
    private readonly string _connectionString =
        $"Server=(localdb)\\MSSQLLocalDB;Database=SOC2Db_Test_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True";

    public TicketTestDatabase()
    {
        using var db = NieuweContext();
        db.Database.EnsureDeleted();   // oude testdata weg
        db.Database.Migrate();         // alle migraties uitvoeren, inclusief de trigger
    }

    // Elke keer een nieuwe DbContext, net als bij een echt HTTP-verzoek
    public SOC2DBContext NieuweContext() => new(new DbContextOptionsBuilder<SOC2DBContext>()
        .UseSqlServer(_connectionString).Options);

    public void Dispose()
    {
        using var db = NieuweContext();
        db.Database.EnsureDeleted();   // na de tests opruimen
    }
}