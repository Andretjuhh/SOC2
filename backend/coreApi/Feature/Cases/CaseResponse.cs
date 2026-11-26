using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Cases;

// Een ticket zoals het in het casedossier wordt getoond
public sealed record CaseTicketResponse(
    int TicketID,
    string TicketNummer,
    string Titel,
    TicketPrioriteit Prioriteit,
    TicketStatus Status);

// Wat de API teruggeeft over een case, inclusief de gekoppelde tickets
public sealed record CaseResponse(
    int CaseID,
    string CaseNummer,
    string Titel,
    string Beschrijving,
    TicketPrioriteit Prioriteit,
    CaseStatus Status,
    string? RootCause,
    string? AanvalsTijdlijn,
    Guid AangemaaktDoor,
    string AangemaaktDoorNaam,
    DateTime AangemaaktOp,
    DateTime? AfgeslotenOp,
    byte[] Versie,
    List<CaseTicketResponse> Tickets)
{
    public static string Nummer(int caseId) => $"CASE-{caseId:D4}";   // bijv. CASE-0001

    // Haalt één case van de eigen klant op met tickets. Null = bestaat niet of hoort bij een andere klant.
    public static async Task<CaseResponse?> LaadAsync(SOC2DBContext db, int caseId, Guid? klantId, CancellationToken ct)
    {
        var zaak = await db.Cases.AsNoTracking()
            .Include(x => x.Tickets)
            .SingleOrDefaultAsync(x => x.CaseID == caseId && x.KlantID == klantId, ct);
        if (zaak is null)
            return null;

        var aanmaker = await db.Gebruikers.AsNoTracking()
            .Where(g => g.Id == zaak.AangemaaktDoor)
            .Select(g => g.Naam)
            .FirstOrDefaultAsync(ct);

        var tickets = zaak.Tickets
            .OrderBy(t => t.TicketID)
            .Select(t => new CaseTicketResponse(t.TicketID, TicketResponse.Nummer(t.TicketID), t.Titel, t.Prioriteit, t.Status))
            .ToList();

        return new CaseResponse(
            zaak.CaseID,
            Nummer(zaak.CaseID),
            zaak.Titel,
            zaak.Beschrijving,
            zaak.Prioriteit,
            zaak.Status,
            zaak.RootCause,
            zaak.AanvalsTijdlijn,
            zaak.AangemaaktDoor,
            aanmaker ?? "Onbekende gebruiker",
            zaak.AangemaaktOp,
            zaak.AfgeslotenOp,
            zaak.Versie,
            tickets);
    }

    // Meldingen die op meerdere plekken nodig zijn
    public static IResult NietGevonden() => Results.NotFound(new { melding = "Case niet gevonden." });

    public static IResult ConflictMelding() => Results.Problem(statusCode: 409,
        title: "Deze case is tussentijds gewijzigd door een andere gebruiker. Ververs de pagina om de meest actuele gegevens te zien.");
}
