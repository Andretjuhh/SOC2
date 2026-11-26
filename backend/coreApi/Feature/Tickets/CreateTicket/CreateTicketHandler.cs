using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets.CreateTicket;

public static class CreateTicketHandler
{
    public static async Task<IResult> Handle(CreateTicketRequest request, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        // 1. Wie is ingelogd, en mag die tickets aanmaken?
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagSchrijven(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om tickets aan te maken.");

        // 2. Invoer controleren
        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["ticket"] = [fout] });

        // 3. Komt het ticket uit een alert? Dan moet die alert bij de eigen klant horen
        //    en mag er nog geen ticket voor bestaan (één alert = hoogstens één ticket).
        if (request.AlertID is not null)
        {
            var alertBestaat = await db.Alerts
                .AnyAsync(x => x.AlertID == request.AlertID && x.KlantID == gebruiker.KlantID, ct);
            if (!alertBestaat)
                return Results.NotFound(new { melding = "Alert niet gevonden." });

            var bestaandTicket = await db.Tickets
                .Where(x => x.AlertID == request.AlertID)
                .Select(x => (int?)x.TicketID)
                .FirstOrDefaultAsync(ct);
            if (bestaandTicket is not null)
                return Results.Problem(statusCode: 409,
                    title: $"Voor deze alert bestaat al ticket {TicketResponse.Nummer(bestaandTicket.Value)}.");
        }

        // 4. Ticket maken, altijd met status "Open"
        var ticket = new Ticket
        {
            AlertID = request.AlertID,
            Titel = request.Titel.Trim(),
            Beschrijving = request.Beschrijving.Trim(),
            Prioriteit = request.Prioriteit,
            Categorie = request.Categorie,
            Status = TicketStatus.Open,
            KlantID = gebruiker.KlantID!.Value,
            AangemaaktDoor = gebruiker.Id
        };

        // 5. Historie-regel toevoegen (wordt in dezelfde transactie opgeslagen)
        ticket.Historie.Add(new TicketHistorie
        {
            Actie = "Aangemaakt",
            Omschrijving = $"Ticket aangemaakt met status {ticket.Status}.",
            GebruikerID = gebruiker.Id
        });

        // Bevindingen al ingevuld bij het claimen? Dan wordt er meteen aan gewerkt.
        if (TicketRegels.PasStatusAan(ticket))
        {
            ticket.Historie.Add(new TicketHistorie
            {
                Actie = "Bijgewerkt",
                Omschrijving = "Status: Open -> InBehandeling (automatisch: bevindingen ingevuld bij aanmaken).",
                GebruikerID = gebruiker.Id
            });
        }

        db.Tickets.Add(ticket);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) when (request.AlertID is not null)
        {
            // Twee analisten claimden tegelijk: de unieke index op AlertID houdt de tweede tegen
            return Results.Problem(statusCode: 409, title: "Voor deze alert is net al een ticket aangemaakt.");
        }

        return Results.Created($"/api/tickets/{ticket.TicketID}", TicketResponse.Van(ticket));
    }
}