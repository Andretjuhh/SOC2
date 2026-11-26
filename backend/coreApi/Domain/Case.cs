namespace CoreApi.Domain;

public enum CaseStatus
{
    Open,
    InOnderzoek,
    Afgesloten
}

// Een case bundelt een of meer tickets die bij hetzelfde incident horen.
// Een ticket hoort bij hoogstens één case (zie Ticket.CaseID).
public class Case
{
    // Uniek casenummer, de database telt automatisch op (wordt getoond als CASE-0001)
    public int CaseID { get; set; }

    public string Titel { get; set; } = string.Empty;
    public string Beschrijving { get; set; } = string.Empty;

    // Prioriteit gebruikt dezelfde schaal als tickets
    public TicketPrioriteit Prioriteit { get; set; }

    // Een nieuwe case begint altijd als "Open"
    public CaseStatus Status { get; set; } = CaseStatus.Open;

    // Onderzoeksdossier: wordt tijdens het onderzoek ingevuld
    public string? RootCause { get; set; }

    // Bij welke klant (tenant) hoort deze case
    public Guid KlantID { get; set; }
    public Klant Klant { get; set; } = null!;

    // Wie heeft de case aangemaakt en wanneer
    public Guid AangemaaktDoor { get; set; }
    public DateTime AangemaaktOp { get; set; } = DateTime.UtcNow;

    public DateTime? AfgeslotenOp { get; set; }

    // Versienummer voor gelijktijdig bewerken, net als bij Ticket
    public byte[] Versie { get; set; } = [];

    // Alle tickets die naar deze case zijn gepromoveerd
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    // Aanvalstijdlijn: handmatig toegevoegde gebeurtenissen (alerts komen er automatisch bij)
    public ICollection<CaseTijdlijnItem> Tijdlijn { get; set; } = new List<CaseTijdlijnItem>();
}
