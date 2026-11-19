using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;

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

        // 3. Ticket maken, altijd met status "Open"
        var ticket = new Ticket
        {
            Titel = request.Titel.Trim(),
            Beschrijving = request.Beschrijving.Trim(),
            Prioriteit = request.Prioriteit,
            Categorie = request.Categorie,
            Status = TicketStatus.Open,
            KlantID = gebruiker.KlantID!.Value,
            AangemaaktDoor = gebruiker.Id
        };

        // 4. Historie-regel toevoegen (wordt in dezelfde transactie opgeslagen)
        ticket.Historie.Add(new TicketHistorie
        {
            Actie = "Aangemaakt",
            Omschrijving = $"Ticket aangemaakt met status {ticket.Status}.",
            GebruikerID = gebruiker.Id
        });

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/tickets/{ticket.TicketID}", TicketResponse.Van(ticket));
    }
}