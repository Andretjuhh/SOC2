using CoreApi.Domain;

namespace CoreApi.Features.Tickets;

// Wat de API teruggeeft over een ticket
public sealed record TicketResponse(
    int TicketID,
    string TicketNummer,
    string Titel,
    string Beschrijving,
    TicketPrioriteit Prioriteit,
    TicketCategorie Categorie,
    TicketStatus Status,
    string? Oplossing,
    Guid AangemaaktDoor,
    DateTime AangemaaktOp,
    DateTime? AfgerondOp,
    byte[] Versie)
{
    public static TicketResponse Van(Ticket ticket) => new(
        ticket.TicketID,
        $"SOC-{ticket.TicketID:D6}",   // bijv. SOC-000001
        ticket.Titel,
        ticket.Beschrijving,
        ticket.Prioriteit,
        ticket.Categorie,
        ticket.Status,
        ticket.Oplossing,
        ticket.AangemaaktDoor,
        ticket.AangemaaktOp,
        ticket.AfgerondOp,
        ticket.Versie);
}
