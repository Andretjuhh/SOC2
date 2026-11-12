using System.Security.Claims;
using CoreApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets.GetTicket;

public static class GetTicketHandler
{
    public static async Task<IResult> Handle(int id, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagLezen(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om tickets te bekijken.");

        // Alleen tickets van de eigen klant. Een ticket van een andere klant geeft 404.
        var ticket = await db.Tickets.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TicketID == id && x.KlantID == gebruiker.KlantID, ct);

        if (ticket is null)
            return Results.NotFound(new { melding = "Ticket niet gevonden." });

        return Results.Ok(TicketResponse.Van(ticket));
    }
}