using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class GebruikerConfiguration : IEntityTypeConfiguration<Gebruiker>
{
    public void Configure(EntityTypeBuilder<Gebruiker> builder)
    {
        builder.ToTable("Gebruikers");
        builder.Property(gebruiker => gebruiker.Id).HasColumnName("GebruikerID");
        builder.Property(gebruiker => gebruiker.UserName).HasColumnName("Gebruikersnaam")
            .HasMaxLength(100).IsRequired();
        builder.Property(gebruiker => gebruiker.NormalizedUserName).HasColumnName("GenormaliseerdeGebruikersnaam")
            .HasMaxLength(100).IsRequired();
        builder.Property(gebruiker => gebruiker.PasswordHash).HasColumnName("WachtwoordHash");
        builder.HasIndex(gebruiker => gebruiker.NormalizedUserName)
            .HasDatabaseName("IX_Gebruikers_GenormaliseerdeGebruikersnaam").IsUnique();
        builder.HasOne(gebruiker => gebruiker.Klant).WithMany(klant => klant.Gebruikers)
            .HasForeignKey(gebruiker => gebruiker.KlantID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(gebruiker => gebruiker.Rol).WithMany(rol => rol.Gebruikers)
            .HasForeignKey(gebruiker => gebruiker.RolID).OnDelete(DeleteBehavior.Restrict);
    }
}
