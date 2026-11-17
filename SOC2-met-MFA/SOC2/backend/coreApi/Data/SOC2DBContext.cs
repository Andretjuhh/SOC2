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
    }
}
