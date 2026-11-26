using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets.ListTickets;

// Eén regel in het ticketoverzicht: alle ticketgegevens plus wie het ticket heeft geclaimd/aangemaakt.
public sealed record TicketOverzichtResponse(
    int TicketID,
    string TicketNummer,
    string Titel,
    string Beschrijving,
    TicketPrioriteit Prioriteit,
    TicketCategorie Categorie,
    TicketStatus Status,
    string? Oplossing,
    Guid AangemaaktDoor,
    string AangemaaktDoorNaam,
    string AangemaaktDoorGebruikersnaam,
    DateTime AangemaaktOp,
    DateTime? AfgerondOp,
    DateTime LaatstGewijzigdOp,
    byte[] Versie);

public static class ListTicketsHandler
{
    public static async Task<IResult> Handle(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagLezen(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om tickets te bekijken.");

        // Alleen tickets van de eigen klant, nieuwste eerst
        var regels = await db.Tickets.AsNoTracking()
            .Where(x => (RechtenMatrix.IsBeheerder(gebruiker) || x.KlantID == gebruiker.KlantID))
            .OrderByDescending(x => x.AangemaaktOp)
            .Select(x => new
            {
                Ticket = x,
                Aanmaker = db.Gebruikers
                    .Where(g => g.Id == x.AangemaaktDoor)
                    .Select(g => new { g.Naam, g.UserName })
                    .FirstOrDefault(),
                LaatsteWijziging = x.Historie.Max(h => (DateTime?)h.Tijdstip)
            })
            .ToListAsync(ct);

        var tickets = regels.Select(regel =>
        {
            var ticket = TicketResponse.Van(regel.Ticket);
            return new TicketOverzichtResponse(
                ticket.TicketID,
                ticket.TicketNummer,
                ticket.Titel,
                ticket.Beschrijving,
                ticket.Prioriteit,
                ticket.Categorie,
                ticket.Status,
                ticket.Oplossing,
                ticket.AangemaaktDoor,
                regel.Aanmaker?.Naam ?? "Onbekende gebruiker",
                regel.Aanmaker?.UserName ?? "onbekend",
                ticket.AangemaaktOp,
                ticket.AfgerondOp,
                regel.LaatsteWijziging ?? ticket.AangemaaktOp,
                ticket.Versie);
        }).ToList();

        return Results.Ok(tickets);
    }
}
