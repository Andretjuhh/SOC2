using System.Security.Claims;
using CoreApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets.GetTicketHistorie;

public sealed record TicketHistorieResponse(string Actie, string Omschrijving, Guid GebruikerID, DateTime Tijdstip,
    string GebruikerNaam, string Gebruikersnaam);

public static class GetTicketHistorieHandler
{
    public static async Task<IResult> Handle(int id, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagLezen(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om tickets te bekijken.");

        // Bestaat het ticket bij de eigen klant?
        var bestaat = await db.Tickets.AnyAsync(x => x.TicketID == id && x.KlantID == gebruiker.KlantID, ct);
        if (!bestaat)
            return Results.NotFound(new { melding = "Ticket niet gevonden." });

        var historie = await db.TicketHistorie.AsNoTracking()
            .Where(x => x.TicketID == id)
            .OrderBy(x => x.Tijdstip)
            .Select(x => new TicketHistorieResponse(x.Actie, x.Omschrijving, x.GebruikerID, x.Tijdstip,
                db.Gebruikers.Where(g => g.Id == x.GebruikerID).Select(g => g.Naam).FirstOrDefault()
                    ?? "Onbekende gebruiker",
                db.Gebruikers.Where(g => g.Id == x.GebruikerID).Select(g => g.UserName).FirstOrDefault()
                    ?? "onbekend"))
            .ToListAsync(ct);

        return Results.Ok(historie);
    }
}