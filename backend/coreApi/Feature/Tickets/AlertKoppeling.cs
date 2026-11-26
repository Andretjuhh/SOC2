using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets;

// Koppelt tickets van vóór de AlertID-kolom alsnog aan hun alert.
// Zulke tickets hebben de alertgegevens alleen als tekst in de beschrijving (zie maakTicketVanAlert in de frontend):
//   Aangemaakt vanuit alert: <event>
//   Ernst: <severity>
//   Getroffen asset: <asset>
//   Gedetecteerd: <tijd, bijv. 09:01:23>
// We koppelen alleen als er precies één alert past, anders laten we het ticket ongemoeid.
public static class AlertKoppeling
{
    public static async Task<int> KoppelOudeTicketsAsync(SOC2DBContext db, CancellationToken ct = default)
    {
        var tickets = await db.Tickets
            .Where(t => t.AlertID == null && t.Beschrijving.Contains("Aangemaakt vanuit alert:"))
            .ToListAsync(ct);
        if (tickets.Count == 0)
            return 0;

        // Alerts die al aan een ticket hangen, mogen niet nog een keer gekoppeld worden
        var bezet = (await db.Tickets.Where(t => t.AlertID != null).Select(t => t.AlertID!.Value).ToListAsync(ct)).ToHashSet();
        var klantIds = tickets.Select(t => t.KlantID).Distinct().ToList();
        var alerts = await db.Alerts.AsNoTracking().Where(a => klantIds.Contains(a.KlantID)).ToListAsync(ct);

        var gekoppeld = 0;
        foreach (var ticket in tickets.OrderBy(t => t.AangemaaktOp))
        {
            var gegevens = LeesAlertGegevens(ticket.Beschrijving);
            if (gegevens is null)
                continue;

            // Zelfde klant en dezelfde gegevens, gedetecteerd vóór het ticket, en geclaimd door wie het ticket maakte
            var kandidaten = alerts.Where(a =>
                    !bezet.Contains(a.AlertID)
                    && a.KlantID == ticket.KlantID
                    && a.Event == gegevens.Event
                    && a.Severity == gegevens.Severity
                    && a.AffectedAsset == gegevens.Asset
                    && a.DetectionTime <= ticket.AangemaaktOp
                    && (a.AssignedUserId == null || a.AssignedUserId == ticket.AangemaaktDoor))
                .ToList();

            // Meerdere kandidaten? Dan moet ook de detectietijd uit de tekst kloppen
            if (kandidaten.Count > 1 && gegevens.Tijd is not null)
                kandidaten = kandidaten.Where(a => LokaleTijd(a.DetectionTime) == gegevens.Tijd).ToList();

            if (kandidaten.Count != 1)
                continue;

            var alert = kandidaten[0];
            ticket.AlertID = alert.AlertID;
            bezet.Add(alert.AlertID);
            ticket.Historie.Add(new TicketHistorie
            {
                Actie = "Bijgewerkt",
                Omschrijving = $"Gekoppeld aan alert #{alert.AlertID} (automatisch, op basis van de alertgegevens in de beschrijving).",
                GebruikerID = Guid.Empty   // door het systeem
            });
            gekoppeld++;
        }

        await db.SaveChangesAsync(ct);
        return gekoppeld;
    }

    private sealed record AlertGegevens(string Event, string Severity, string Asset, string? Tijd);

    private static AlertGegevens? LeesAlertGegevens(string beschrijving)
    {
        var regels = beschrijving.Replace("\r\n", "\n").Split('\n').Select(r => r.Trim()).ToList();

        string? Waarde(string kop) => regels
            .FirstOrDefault(r => r.StartsWith(kop, StringComparison.Ordinal))?[kop.Length..].Trim();

        var gebeurtenis = Waarde("Aangemaakt vanuit alert:");
        var ernst = Waarde("Ernst:");
        var asset = Waarde("Getroffen asset:");
        if (string.IsNullOrEmpty(gebeurtenis) || string.IsNullOrEmpty(ernst) || asset is null)
            return null;

        return new AlertGegevens(gebeurtenis, ernst, asset, Waarde("Gedetecteerd:"));
    }

    // De frontend schreef de detectietijd als Nederlandse kloktijd (bijv. 09:01:23)
    private static string LokaleTijd(DateTime utc)
    {
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam"); }
        catch (TimeZoneNotFoundException) { zone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); }

        var lokaal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);
        return lokaal.ToString("HH:mm:ss");
    }
}
