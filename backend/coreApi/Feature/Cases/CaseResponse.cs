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

// Eén regel in de aanvalstijdlijn.
// Bron "Alert": automatisch uit een alert van een gekoppeld ticket (ItemID is dan leeg, niet te verwijderen).
// Bron "Handmatig": door een analist toegevoegd.
public sealed record TijdlijnItemResponse(
    int? ItemID,
    DateTime Tijdstip,
    string Beschrijving,
    string? Systeem,
    AanvalsFase? Fase,
    string Bron,
    string? TicketNummer,
    string? ToegevoegdDoorNaam);

// Wat de API teruggeeft over een case, inclusief de gekoppelde tickets en de aanvalstijdlijn
public sealed record CaseResponse(
    int CaseID,
    string CaseNummer,
    string Titel,
    string Beschrijving,
    TicketPrioriteit Prioriteit,
    CaseStatus Status,
    string? RootCause,
    Guid AangemaaktDoor,
    string AangemaaktDoorNaam,
    DateTime AangemaaktOp,
    DateTime? AfgeslotenOp,
    byte[] Versie,
    List<CaseTicketResponse> Tickets,
    List<TijdlijnItemResponse> Tijdlijn)
{
    public static string Nummer(int caseId) => $"CASE-{caseId:D4}";   // bijv. CASE-0001

    // Haalt één case van de eigen klant op met tickets en tijdlijn. Null = bestaat niet of hoort bij een andere klant.
    public static async Task<CaseResponse?> LaadAsync(SOC2DBContext db, int caseId, Guid? klantId, CancellationToken ct)
    {
        var zaak = await db.Cases.AsNoTracking()
            .Include(x => x.Tickets)
            .Include(x => x.Tijdlijn)
            .SingleOrDefaultAsync(x => x.CaseID == caseId && x.KlantID == klantId, ct);
        if (zaak is null)
            return null;

        // Namen van iedereen die we tonen: de aanmaker van de case en wie gebeurtenissen toevoegde
        var gebruikerIds = zaak.Tijdlijn.Select(x => x.ToegevoegdDoor).Append(zaak.AangemaaktDoor).Distinct().ToList();
        var namen = await db.Gebruikers.AsNoTracking()
            .Where(g => gebruikerIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id, g => g.Naam, ct);

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
            zaak.AangemaaktDoor,
            namen.GetValueOrDefault(zaak.AangemaaktDoor) ?? "Onbekende gebruiker",
            zaak.AangemaaktOp,
            zaak.AfgeslotenOp,
            zaak.Versie,
            tickets,
            await BouwTijdlijnAsync(db, zaak, namen, ct));
    }

    // Alerts van de gekoppelde tickets + handmatige gebeurtenissen, gesorteerd op het moment dat het gebeurde
    private static async Task<List<TijdlijnItemResponse>> BouwTijdlijnAsync(SOC2DBContext db, Case zaak,
        Dictionary<Guid, string> namen, CancellationToken ct)
    {
        var ticketPerAlert = zaak.Tickets
            .Where(t => t.AlertID is not null)
            .ToDictionary(t => t.AlertID!.Value, t => t.TicketID);
        var alertIds = ticketPerAlert.Keys.ToList();

        var alerts = await db.Alerts.AsNoTracking()
            .Where(a => alertIds.Contains(a.AlertID) && a.KlantID == zaak.KlantID)
            .ToListAsync(ct);

        var uitAlerts = alerts.Select(a => new TijdlijnItemResponse(
            null,
            AlsUtc(a.DetectionTime),
            $"Alert gedetecteerd: {a.Event} ({a.Severity})",
            a.AffectedAsset,
            null,
            "Alert",
            TicketResponse.Nummer(ticketPerAlert[a.AlertID]),
            null));

        var handmatig = zaak.Tijdlijn.Select(x => new TijdlijnItemResponse(
            x.CaseTijdlijnItemID,
            AlsUtc(x.Tijdstip),
            x.Beschrijving,
            x.Systeem,
            x.Fase,
            "Handmatig",
            null,
            namen.GetValueOrDefault(x.ToegevoegdDoor) ?? "Onbekende gebruiker"));

        return uitAlerts.Concat(handmatig).OrderBy(x => x.Tijdstip).ToList();
    }

    // SQL Server bewaart geen tijdzone; alle tijden in de database zijn UTC
    private static DateTime AlsUtc(DateTime waarde) => DateTime.SpecifyKind(waarde, DateTimeKind.Utc);

    // Meldingen die op meerdere plekken nodig zijn
    public static IResult NietGevonden() => Results.NotFound(new { melding = "Case niet gevonden." });

    public static IResult ConflictMelding() => Results.Problem(statusCode: 409,
        title: "Deze case is tussentijds gewijzigd door een andere gebruiker. Ververs de pagina om de meest actuele gegevens te zien.");
}
