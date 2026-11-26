using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Cases.UpdateCase;

public static class UpdateCaseHandler
{
    public static async Task<IResult> Handle(int id, UpdateCaseRequest request, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        // 1. Wie is ingelogd, en mag die cases bewerken?
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagSchrijven(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om cases te bewerken.");

        // 2. Case ophalen (alleen van de eigen klant)
        var zaak = await db.Cases.SingleOrDefaultAsync(x => x.CaseID == id && x.KlantID == gebruiker.KlantID, ct);
        if (zaak is null)
            return CaseResponse.NietGevonden();

        // 3. Een afgesloten case is op slot
        if (zaak.Status == CaseStatus.Afgesloten)
            return Results.Problem(statusCode: 409, title: "Een afgesloten case kan niet meer worden bewerkt.");

        // 4. Invoer en versie controleren
        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["case"] = [fout] });
        if (!zaak.Versie.SequenceEqual(request.Versie!))
            return CaseResponse.ConflictMelding();

        // 5. Aanpassen wat is meegestuurd. Lege dossiervelden worden opgeslagen als 'niet ingevuld'.
        if (request.Titel is not null) zaak.Titel = request.Titel.Trim();
        if (request.Beschrijving is not null) zaak.Beschrijving = request.Beschrijving.Trim();
        if (request.Prioriteit is not null) zaak.Prioriteit = request.Prioriteit.Value;
        if (request.RootCause is not null) zaak.RootCause = LeegNaarNull(request.RootCause);

        if (request.Status is not null && request.Status != zaak.Status)
        {
            // Afsluiten kan pas als de oorzaak bekend is
            if (request.Status == CaseStatus.Afgesloten && string.IsNullOrWhiteSpace(zaak.RootCause))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["rootCause"] = ["Vul de root cause in voordat je de case afsluit."] });

            zaak.Status = request.Status.Value;
            if (zaak.Status == CaseStatus.Afgesloten)
                zaak.AfgeslotenOp = DateTime.UtcNow;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CaseResponse.ConflictMelding();
        }

        return Results.Ok(await CaseResponse.LaadAsync(db, zaak.CaseID, gebruiker.KlantID, ct));
    }

    private static string? LeegNaarNull(string waarde) =>
        string.IsNullOrWhiteSpace(waarde) ? null : waarde.Trim();
}
