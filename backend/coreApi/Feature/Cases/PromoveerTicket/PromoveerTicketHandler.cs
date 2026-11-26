using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Cases.PromoveerTicket;

public static class PromoveerTicketHandler
{
    public static async Task<IResult> Handle(int id, PromoveerTicketRequest request, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        // 1. Wie is ingelogd, en mag die tickets wijzigen?
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagSchrijven(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om tickets te escaleren.");

        // 2. Ticket ophalen (alleen van de eigen klant)
        var ticket = await db.Tickets
            .SingleOrDefaultAsync(x => x.TicketID == id && x.KlantID == gebruiker.KlantID, ct);
        if (ticket is null)
            return Results.NotFound(new { melding = "Ticket niet gevonden." });

        // 3. Dubbele promotie voorkomen: een ticket hoort bij hoogstens één case
        if (ticket.CaseID is not null)
            return Results.Problem(statusCode: 409,
                title: $"Dit ticket is al geëscaleerd naar {CaseResponse.Nummer(ticket.CaseID.Value)}.");
        if (ticket.Status == TicketStatus.Afgerond)
            return Results.Problem(statusCode: 409, title: "Een afgerond ticket kan niet meer worden geëscaleerd.");

        // 4. Invoer en versie controleren
        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["case"] = [fout] });
        if (!ticket.Versie.SequenceEqual(request.Versie!))
            return TicketToegang.ConflictMelding();

        // 5. Alles in één transactie: lukt één stap niet, dan wordt niets opgeslagen
        await using var transactie = await db.Database.BeginTransactionAsync(ct);

        Case zaak;
        string actie;
        if (request.CaseID is not null)
        {
            // Bestaande case: moet van dezelfde klant zijn en nog open staan
            var bestaand = await db.Cases
                .SingleOrDefaultAsync(x => x.CaseID == request.CaseID && x.KlantID == gebruiker.KlantID, ct);
            if (bestaand is null)
                return CaseResponse.NietGevonden();
            if (bestaand.Status == CaseStatus.Afgesloten)
                return Results.Problem(statusCode: 409, title: "Aan een afgesloten case kun je geen tickets meer toevoegen.");

            zaak = bestaand;
            actie = "toegevoegd aan";
        }
        else
        {
            zaak = new Case
            {
                Titel = request.Titel!.Trim(),
                Beschrijving = request.Beschrijving!.Trim(),
                Prioriteit = request.Prioriteit!.Value,
                Status = CaseStatus.Open,
                KlantID = gebruiker.KlantID!.Value,
                AangemaaktDoor = gebruiker.Id
            };
            db.Cases.Add(zaak);
            // Eerst opslaan zodat de case een nummer krijgt voor de historie-regel
            await db.SaveChangesAsync(ct);
            actie = "geëscaleerd naar nieuwe";
        }

        // 6. Ticket koppelen; de status van het ticket blijft gewoon staan
        ticket.CaseID = zaak.CaseID;
        ticket.Historie.Add(new TicketHistorie
        {
            Actie = "Geëscaleerd",
            Omschrijving = $"Ticket {actie} case {CaseResponse.Nummer(zaak.CaseID)}.",
            GebruikerID = gebruiker.Id
        });

        try
        {
            await db.SaveChangesAsync(ct);
            await transactie.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Zonder commit wordt de transactie teruggedraaid, ook een net aangemaakte case
            return TicketToegang.ConflictMelding();
        }

        var resultaat = await CaseResponse.LaadAsync(db, zaak.CaseID, gebruiker.KlantID, ct);
        return Results.Ok(resultaat);
    }
}
