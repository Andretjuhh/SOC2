using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets.UpdateTicket;

public static class UpdateTicketHandler
{
    public static async Task<IResult> Handle(int id, UpdateTicketRequest request, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        // 1. Wie is ingelogd, en mag die tickets bewerken?
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagSchrijven(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om tickets te bewerken.");

        // 2. Ticket ophalen (alleen van de eigen klant)
        var ticket = await db.Tickets
            .SingleOrDefaultAsync(x => x.TicketID == id && x.KlantID == gebruiker.KlantID, ct);
        if (ticket is null)
            return Results.NotFound(new { melding = "Ticket niet gevonden." });

        // 3. Een afgerond ticket is op slot
        if (ticket.Status == TicketStatus.Afgerond)
            return Results.Problem(statusCode: 409, title: "Een afgerond ticket kan niet meer worden bewerkt.");

        // 4. Invoer controleren
        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["ticket"] = [fout] });

        // 5. Alleen aanpassen wat echt anders is, en bijhouden wat er veranderd is
        var wijzigingen = new List<string>();

        if (request.Titel is not null && request.Titel.Trim() != ticket.Titel)
        {
            wijzigingen.Add($"Titel: '{ticket.Titel}' -> '{request.Titel.Trim()}'");
            ticket.Titel = request.Titel.Trim();
        }
        if (request.Beschrijving is not null && request.Beschrijving.Trim() != ticket.Beschrijving)
        {
            wijzigingen.Add("Beschrijving gewijzigd");
            ticket.Beschrijving = request.Beschrijving.Trim();
        }
        if (request.Prioriteit is not null && request.Prioriteit != ticket.Prioriteit)
        {
            wijzigingen.Add($"Prioriteit: {ticket.Prioriteit} -> {request.Prioriteit}");
            ticket.Prioriteit = request.Prioriteit.Value;
        }
        if (request.Categorie is not null && request.Categorie != ticket.Categorie)
        {
            wijzigingen.Add($"Categorie: {ticket.Categorie} -> {request.Categorie}");
            ticket.Categorie = request.Categorie.Value;
        }
        if (request.Status is not null && request.Status != ticket.Status)
        {
            wijzigingen.Add($"Status: {ticket.Status} -> {request.Status}");
            ticket.Status = request.Status.Value;
        }

        // Niets veranderd? Dan ook geen historie-regel
        if (wijzigingen.Count == 0)
            return Results.Ok(TicketResponse.Van(ticket));

        // 6. Historie-regel toevoegen en alles in één keer opslaan
        ticket.Historie.Add(new TicketHistorie
        {
            Actie = "Bijgewerkt",
            Omschrijving = string.Join("; ", wijzigingen),
            GebruikerID = gebruiker.Id
        });

        await db.SaveChangesAsync(ct);

        return Results.Ok(TicketResponse.Van(ticket));
    }
}