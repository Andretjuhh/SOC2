using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class RechtConfiguration : IEntityTypeConfiguration<Recht>
{
    public void Configure(EntityTypeBuilder<Recht> builder)
    {
        builder.HasKey(recht => recht.RechtID);
        builder.Property(recht => recht.Naam).HasMaxLength(100);
        builder.HasIndex(recht => recht.Naam).IsUnique();
    }
}
