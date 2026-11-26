using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class CaseTijdlijnItemConfiguration : IEntityTypeConfiguration<CaseTijdlijnItem>
{
    public void Configure(EntityTypeBuilder<CaseTijdlijnItem> builder)
    {
        builder.ToTable("CaseTijdlijn");
        builder.HasKey(item => item.CaseTijdlijnItemID);
        builder.Property(item => item.Beschrijving).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.Systeem).HasMaxLength(200);
        builder.Property(item => item.Fase).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(item => item.Case).WithMany(zaak => zaak.Tijdlijn)
            .HasForeignKey(item => item.CaseID).OnDelete(DeleteBehavior.Restrict);

        // De tijdlijn wordt altijd per case op tijdstip opgehaald
        builder.HasIndex(item => new { item.CaseID, item.Tijdstip });
    }
}
