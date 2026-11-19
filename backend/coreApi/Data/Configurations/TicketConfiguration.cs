using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreApi.Data.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(ticket => ticket.TicketID);
        builder.Property(ticket => ticket.Titel).HasMaxLength(200).IsRequired();
        builder.Property(ticket => ticket.Beschrijving).IsRequired();

        builder.Property(ticket => ticket.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(ticket => ticket.Prioriteit).HasConversion<string>().HasMaxLength(20);
        builder.Property(ticket => ticket.Categorie).HasConversion<string>().HasMaxLength(50);
        builder.Property(ticket => ticket.Versie).IsRowVersion();

        builder.HasOne(ticket => ticket.Klant).WithMany()
            .HasForeignKey(ticket => ticket.KlantID).OnDelete(DeleteBehavior.Restrict);
    }
}
