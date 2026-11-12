using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.HasKey(alert => alert.AlertID);
        builder.Property(alert => alert.Event).HasMaxLength(200).IsRequired();
        builder.Property(alert => alert.Severity).HasMaxLength(20).IsRequired();
        builder.Property(alert => alert.AffectedAsset).HasMaxLength(200).IsRequired();

        builder.HasOne(alert => alert.Klant).WithMany(klant => klant.Alerts)
            .HasForeignKey(alert => alert.KlantID).OnDelete(DeleteBehavior.Restrict);
    }
}
