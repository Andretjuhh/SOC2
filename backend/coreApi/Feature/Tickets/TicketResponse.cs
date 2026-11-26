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
    byte[] Versie,
    int? CaseID,
    string? CaseNummer,
    int? AlertID)
{
    public static string Nummer(int ticketId) => $"SOC-{ticketId:D6}";   // bijv. SOC-000001

    public static TicketResponse Van(Ticket ticket) => new(
        ticket.TicketID,
        Nummer(ticket.TicketID),
        ticket.Titel,
        ticket.Beschrijving,
        ticket.Prioriteit,
        ticket.Categorie,
        ticket.Status,
        ticket.Oplossing,
        ticket.AangemaaktDoor,
        ticket.AangemaaktOp,
        ticket.AfgerondOp,
        ticket.Versie,
        ticket.CaseID,
        // Afgeleid: een ticket met een CaseID is geëscaleerd naar die case
        ticket.CaseID is null ? null : $"CASE-{ticket.CaseID.Value:D4}",
        ticket.AlertID);
}
