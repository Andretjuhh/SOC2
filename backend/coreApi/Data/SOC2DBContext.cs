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

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.Entity<Gebruiker>(entity =>
        {
            entity.ToTable("Gebruikers");
            entity.Property(gebruiker => gebruiker.Id).HasColumnName("GebruikerID");
            entity.Property(gebruiker => gebruiker.UserName).HasColumnName("Gebruikersnaam").HasMaxLength(100).IsRequired();
            entity.Property(gebruiker => gebruiker.NormalizedUserName).HasColumnName("GenormaliseerdeGebruikersnaam")
                .HasMaxLength(100).IsRequired();
            entity.Property(gebruiker => gebruiker.PasswordHash).HasColumnName("WachtwoordHash");
            entity.HasIndex(gebruiker => gebruiker.NormalizedUserName)
                .HasDatabaseName("IX_Gebruikers_GenormaliseerdeGebruikersnaam").IsUnique();
            entity.HasOne(gebruiker => gebruiker.Klant).WithMany(klant => klant.Gebruikers)
                .HasForeignKey(gebruiker => gebruiker.KlantID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(gebruiker => gebruiker.Rol).WithMany(rol => rol.Gebruikers)
                .HasForeignKey(gebruiker => gebruiker.RolID).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<Klant>().HasKey(klant => klant.KlantID);
        model.Entity<Rol>().HasKey(rol => rol.RolID);
        model.Entity<Recht>().HasKey(recht => recht.RechtID);
        model.Entity<Rol>().Property(rol => rol.Naam).HasMaxLength(100);
        model.Entity<Rol>().HasIndex(rol => rol.Naam).IsUnique();
        model.Entity<Recht>().Property(recht => recht.Naam).HasMaxLength(100);
        model.Entity<Recht>().HasIndex(recht => recht.Naam).IsUnique();
        model.Entity<Rol>().HasMany(rol => rol.Rechten).WithMany(recht => recht.Rollen);

            model.Entity<Ticket>(entity =>
        {
            entity.HasKey(ticket => ticket.TicketID);
            entity.Property(ticket => ticket.Titel).HasMaxLength(200).IsRequired();
            entity.Property(ticket => ticket.Beschrijving).IsRequired();

            // Enums als tekst opslaan, dan zie je "Open" in de database in plaats van 0
            entity.Property(ticket => ticket.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(ticket => ticket.Prioriteit).HasConversion<string>().HasMaxLength(20);
            entity.Property(ticket => ticket.Categorie).HasConversion<string>().HasMaxLength(50);

            // Versie-kolom (rowversion) voor optimistic locking
            entity.Property(ticket => ticket.Versie).IsRowVersion();

            entity.HasOne(ticket => ticket.Klant).WithMany()
                .HasForeignKey(ticket => ticket.KlantID).OnDelete(DeleteBehavior.Restrict);
        });

            model.Entity<TicketHistorie>(entity =>
        {
            entity.ToTable("TicketHistorie");
            entity.HasKey(historie => historie.TicketHistorieID);
            entity.Property(historie => historie.Actie).HasMaxLength(50).IsRequired();
            entity.Property(historie => historie.Omschrijving).IsRequired();

            entity.HasOne(historie => historie.Ticket).WithMany(ticket => ticket.Historie)
                .HasForeignKey(historie => historie.TicketID).OnDelete(DeleteBehavior.Restrict);

            
        });

        


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
