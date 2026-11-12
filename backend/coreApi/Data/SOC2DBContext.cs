using CoreApi.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Data;

public class SOC2DBContext(DbContextOptions<SOC2DBContext> options) : IdentityUserContext<Gebruiker, Guid>(options)
{
    public DbSet<Gebruiker> Gebruikers => Set<Gebruiker>();
    public DbSet<Klant> Klanten => Set<Klant>();
    public DbSet<Rol> Rollen => Set<Rol>();
    public DbSet<Recht> Rechten => Set<Recht>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketHistorie> TicketHistorie => Set<TicketHistorie>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.ApplyConfigurationsFromAssembly(typeof(SOC2DBContext).Assembly);
    }

    // Beveiliging in de code: historie mag alleen toegevoegd worden, nooit gewijzigd of verwijderd
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ControleerHistorie();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ControleerHistorie();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ControleerHistorie()
    {
        var historieGewijzigd = ChangeTracker.Entries<TicketHistorie>()
            .Any(entry => entry.State == EntityState.Modified || entry.State == EntityState.Deleted);

        if (historieGewijzigd)
            throw new InvalidOperationException("Ticket-historie mag niet worden gewijzigd of verwijderd.");
    }

}
