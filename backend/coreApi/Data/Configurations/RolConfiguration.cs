using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.HasKey(rol => rol.RolID);
        builder.Property(rol => rol.Naam).HasMaxLength(100);
        builder.HasIndex(rol => rol.Naam).IsUnique();
        builder.HasMany(rol => rol.Rechten).WithMany(recht => recht.Rollen);
    }
}
