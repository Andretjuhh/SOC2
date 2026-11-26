using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Cases.AanvalsTijdlijn;

public static class TijdlijnHandlers
{
    // POST /api/cases/{id}/tijdlijn
    public static async Task<IResult> VoegToe(int id, TijdlijnItemRequest request, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        var (fout, zaak, gebruiker) = await HaalBewerkbareCaseOp(id, principal, db, ct);
        if (fout is not null)
            return fout;

        var validatie = request.Valideer();
        if (validatie is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["tijdlijn"] = [validatie] });

        db.CaseTijdlijn.Add(new CaseTijdlijnItem
        {
            CaseID = zaak!.CaseID,
            Tijdstip = request.Tijdstip!.Value.ToUniversalTime(),
            Beschrijving = request.Beschrijving!.Trim(),
            Systeem = string.IsNullOrWhiteSpace(request.Systeem) ? null : request.Systeem.Trim(),
            Fase = request.Fase,
            ToegevoegdDoor = gebruiker!.Id
        });
        await db.SaveChangesAsync(ct);

        return Results.Ok(await CaseResponse.LaadAsync(db, zaak.CaseID, gebruiker.KlantID, ct));
    }

    // DELETE /api/cases/{id}/tijdlijn/{itemId}: om een verkeerd ingevoerde gebeurtenis te corrigeren
    public static async Task<IResult> Verwijder(int id, int itemId, ClaimsPrincipal principal,
        SOC2DBContext db, CancellationToken ct)
    {
        var (fout, zaak, gebruiker) = await HaalBewerkbareCaseOp(id, principal, db, ct);
        if (fout is not null)
            return fout;

        var item = await db.CaseTijdlijn
            .SingleOrDefaultAsync(x => x.CaseTijdlijnItemID == itemId && x.CaseID == zaak!.CaseID, ct);
        if (item is null)
            return Results.NotFound(new { melding = "Gebeurtenis niet gevonden." });

        db.CaseTijdlijn.Remove(item);
        await db.SaveChangesAsync(ct);

        return Results.Ok(await CaseResponse.LaadAsync(db, zaak!.CaseID, gebruiker!.KlantID, ct));
    }

    // Gedeelde controles: ingelogd, schrijfrecht, case van de eigen klant en nog niet afgesloten
    private static async Task<(IResult? Fout, Case? Zaak, Domain.Gebruiker? Gebruiker)> HaalBewerkbareCaseOp(
        int id, ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return (Results.Unauthorized(), null, null);
        if (!TicketToegang.MagSchrijven(gebruiker))
            return (Results.Problem(statusCode: 403, title: "Je hebt geen rechten om de tijdlijn te bewerken."), null, null);

        var zaak = await db.Cases.SingleOrDefaultAsync(x => x.CaseID == id && x.KlantID == gebruiker.KlantID, ct);
        if (zaak is null)
            return (CaseResponse.NietGevonden(), null, null);
        if (zaak.Status == CaseStatus.Afgesloten)
            return (Results.Problem(statusCode: 409, title: "Een afgesloten case kan niet meer worden bewerkt."), null, null);

        return (null, zaak, gebruiker);
    }
}
