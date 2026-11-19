using CoreApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Tests;

// Maakt één keer per testrun een echte, lege SQL Server testdatabase aan (SOC2Db_Test).
// We gebruiken echte SQL Server (geen InMemory) omdat de historie-trigger en de versie-kolom
// alleen in een echte database werken.
public sealed class TicketTestDatabase : IDisposable
{
    private const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=SOC2Db_Test;Trusted_Connection=True;TrustServerCertificate=True";

    public TicketTestDatabase()
    {
        using var db = NieuweContext();
        db.Database.EnsureDeleted();   // oude testdata weg
        db.Database.Migrate();         // alle migraties uitvoeren, inclusief de trigger
    }

    // Elke keer een nieuwe DbContext, net als bij een echt HTTP-verzoek
    public SOC2DBContext NieuweContext() => new(new DbContextOptionsBuilder<SOC2DBContext>()
        .UseSqlServer(ConnectionString).Options);

    public void Dispose()
    {
        using var db = NieuweContext();
        db.Database.EnsureDeleted();   // na de tests opruimen
    }
}