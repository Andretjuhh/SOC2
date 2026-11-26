using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Cases.ListCases;

// Eén regel in het case-overzicht
public sealed record CaseOverzichtResponse(
    int CaseID,
    string CaseNummer,
    string Titel,
    TicketPrioriteit Prioriteit,
    CaseStatus Status,
    int AantalTickets,
    DateTime AangemaaktOp);

public static class ListCasesHandler
{
    public static async Task<IResult> Handle(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        // Cases bestaan uit tickets, dus dezelfde rechten als voor tickets
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagLezen(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om cases te bekijken.");

        // Alleen cases van de eigen klant, nieuwste eerst
        var regels = await db.Cases.AsNoTracking()
            .Where(x => x.KlantID == gebruiker.KlantID)
            .OrderByDescending(x => x.AangemaaktOp)
            .Select(x => new { x.CaseID, x.Titel, x.Prioriteit, x.Status, AantalTickets = x.Tickets.Count, x.AangemaaktOp })
            .ToListAsync(ct);

        var cases = regels.Select(x => new CaseOverzichtResponse(
            x.CaseID, CaseResponse.Nummer(x.CaseID), x.Titel, x.Prioriteit, x.Status, x.AantalTickets, x.AangemaaktOp)).ToList();

        return Results.Ok(cases);
    }
}
