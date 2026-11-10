using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets.ResolveTicket;

public static class ResolveTicketHandler
{
    public static async Task<IResult> Handle(int id, ResolveTicketRequest request, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        // 1. Wie is ingelogd, en mag die tickets afronden?
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagSchrijven(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om tickets af te ronden.");

        // 2. Ticket ophalen (alleen van de eigen klant)
        var ticket = await db.Tickets
            .SingleOrDefaultAsync(x => x.TicketID == id && x.KlantID == gebruiker.KlantID, ct);
        if (ticket is null)
            return Results.NotFound(new { melding = "Ticket niet gevonden." });

        // 3. Al afgerond? Dan mag er niets meer veranderen
        if (ticket.Status == TicketStatus.Afgerond)
            return Results.Problem(statusCode: 409, title: "Dit ticket is al afgerond.");

        // 4. UU6a: zonder oplossing mag je niet afronden, de status blijft dan gewoon hetzelfde

                // UU6b: versie moet meegestuurd worden en nog kloppen
        if (request.Versie is null || request.Versie.Length == 0)
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["versie"] = ["Stuur de versie van het ticket mee."] });
        if (!ticket.Versie.SequenceEqual(request.Versie))
            return TicketToegang.ConflictMelding();

        if (string.IsNullOrWhiteSpace(request.Oplossing))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["oplossing"] = ["Het is verplicht om een oplossingsbeschrijving in te voeren voordat een ticket kan worden afgerond."]
            });

        // 5. Afronden
        var oudeStatus = ticket.Status;
        ticket.Status = TicketStatus.Afgerond;
        ticket.Oplossing = request.Oplossing.Trim();
        ticket.AfgerondOp = DateTime.UtcNow;

        // 6. Historie-regel toevoegen en alles in één keer opslaan
        ticket.Historie.Add(new TicketHistorie
        {
            Actie = "Afgerond",
            Omschrijving = $"Status: {oudeStatus} -> Afgerond. Oplossing: {ticket.Oplossing}",
            GebruikerID = gebruiker.Id
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Iemand anders was ons net voor tussen ophalen en opslaan
            return TicketToegang.ConflictMelding();
        }

        return Results.Ok(TicketResponse.Van(ticket));
    }
}