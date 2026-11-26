using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.HasKey(zaak => zaak.CaseID);
        builder.Property(zaak => zaak.Titel).HasMaxLength(200).IsRequired();
        builder.Property(zaak => zaak.Beschrijving).IsRequired();

        builder.Property(zaak => zaak.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(zaak => zaak.Prioriteit).HasConversion<string>().HasMaxLength(20);
        builder.Property(zaak => zaak.Versie).IsRowVersion();

        builder.HasOne(zaak => zaak.Klant).WithMany()
            .HasForeignKey(zaak => zaak.KlantID).OnDelete(DeleteBehavior.Restrict);
    }
}
