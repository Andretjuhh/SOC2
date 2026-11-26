using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets;

// Statusregel: een ticket waar aan gewerkt wordt, staat niet meer op Open.
// Dat is zo als er bevindingen zijn ingevuld of als het ticket in een case zit.
// Een afgerond ticket blijft altijd afgerond.
public static class TicketRegels
{
    // Moet gelijk zijn aan BEVINDINGEN_KOP in frontend/src/features/tickets/ticketOptions.js
    public const string BevindingenKop = "Bevindingen:";

    public const string OpenNietToegestaan =
        "Een ticket met bevindingen of dat in een case zit, kan niet op Open staan.";

    // Heeft de analist iets ingevuld onder "Bevindingen:"? (alleen tickets uit een alert hebben die kop)
    public static bool HeeftBevindingen(string beschrijving)
    {
        var regels = beschrijving.Replace("\r\n", "\n").Split('\n');
        var index = Array.FindIndex(regels, regel => regel.Trim() == BevindingenKop);
        return index >= 0 && regels.Skip(index + 1).Any(regel => !string.IsNullOrWhiteSpace(regel));
    }

    // Mag dit ticket (nog) op Open staan?
    public static bool MagOpenZijn(Ticket ticket) =>
        ticket.CaseID is null && !HeeftBevindingen(ticket.Beschrijving);

    // Zet een open ticket op In behandeling als de regel dat vraagt. Geeft true als de status is veranderd.
    public static bool PasStatusAan(Ticket ticket)
    {
        if (ticket.Status != TicketStatus.Open || MagOpenZijn(ticket))
            return false;

        ticket.Status = TicketStatus.InBehandeling;
        return true;
    }

    // Eenmalige correctie bij het opstarten: tickets van vóór deze regel die nog ten onrechte op Open staan.
    // Draait elke start, maar doet niets als alles al klopt.
    public static async Task CorrigeerBestaandeTicketsAsync(SOC2DBContext db, CancellationToken ct = default)
    {
        var openTickets = await db.Tickets.Where(x => x.Status == TicketStatus.Open).ToListAsync(ct);

        foreach (var ticket in openTickets.Where(ticket => PasStatusAan(ticket)))
        {
            ticket.Historie.Add(new TicketHistorie
            {
                Actie = "Bijgewerkt",
                Omschrijving = "Status: Open -> InBehandeling (automatisch: ticket heeft bevindingen of zit in een case).",
                GebruikerID = Guid.Empty   // Guid.Empty = door het systeem, niet door een gebruiker
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
