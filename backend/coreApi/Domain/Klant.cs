namespace CoreApi.Domain;

public class Klant
{
    public Guid KlantID { get; set; } = Guid.NewGuid();

    public string naam { get; set; } = string.Empty;

    public string email { get; set; } = string.Empty;

    public string telefoonnummer { get; set; } = string.Empty;

    public string SLA { get; set; } = string.Empty;

    public Boolean isActief { get; set; }

    public ICollection<Gebruiker> Gebruikers { get; set; } = new List<Gebruiker>();

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();

}
