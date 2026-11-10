namespace CoreApi.Domain;

public enum TicketStatus
{
    Open,
    InBehandeling,
    Afgerond
}

public enum TicketPrioriteit
{
    Laag,
    Midden,
    Hoog,
    Kritiek
}

public enum TicketCategorie
{
    Malware,
    Phishing,
    OngeautoriseerdeToegang,
    DDoS,
    Datalek,
    Overig
}

public class Ticket
{
    // Uniek ticketnummer, de database telt automatisch op (1, 2, 3, ...)
    public int TicketID { get; set; }

    public string Titel { get; set; } = string.Empty;
    public string Beschrijving { get; set; } = string.Empty;
    public TicketPrioriteit Prioriteit { get; set; }
    public TicketCategorie Categorie { get; set; }

    // Een nieuw ticket begint altijd als "Open"
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    // Wordt pas ingevuld bij het afronden
    public string? Oplossing { get; set; }

    // Bij welke klant (tenant) hoort dit ticket
    public Guid KlantID { get; set; }
    public Klant Klant { get; set; } = null!;

    // Wie heeft het ticket aangemaakt en wanneer
    public Guid AangemaaktDoor { get; set; }
    public DateTime AangemaaktOp { get; set; } = DateTime.UtcNow;

    public DateTime? AfgerondOp { get; set; }
    
    // Alle historie-regels van dit ticket
    public ICollection<TicketHistorie> Historie { get; set; } = new List<TicketHistorie>();
}