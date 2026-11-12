using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class TicketHistorieConfiguration : IEntityTypeConfiguration<TicketHistorie>
{
    public void Configure(EntityTypeBuilder<TicketHistorie> builder)
    {
        builder.ToTable("TicketHistorie");
        builder.HasKey(historie => historie.TicketHistorieID);
        builder.Property(historie => historie.Actie).HasMaxLength(50).IsRequired();
        builder.Property(historie => historie.Omschrijving).IsRequired();

        builder.HasOne(historie => historie.Ticket).WithMany(ticket => ticket.Historie)
            .HasForeignKey(historie => historie.TicketID).OnDelete(DeleteBehavior.Restrict);
    }
}
