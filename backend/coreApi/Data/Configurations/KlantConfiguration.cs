using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class KlantConfiguration : IEntityTypeConfiguration<Klant>
{
    public void Configure(EntityTypeBuilder<Klant> builder)
    {
        builder.HasKey(klant => klant.KlantID);
    }
}
