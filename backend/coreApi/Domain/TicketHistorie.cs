namespace CoreApi.Domain;

public class TicketHistorie
{
    public int TicketHistorieID { get; set; }

    // Bij welk ticket hoort deze regel
    public int TicketID { get; set; }
    public Ticket Ticket { get; set; } = null!;

    //
    public string Actie { get; set; } = string.Empty;

    
    public string Omschrijving { get; set; } = string.Empty;

    // Wie en wanneer
    public Guid GebruikerID { get; set; }
    public DateTime Tijdstip { get; set; } = DateTime.UtcNow;
}