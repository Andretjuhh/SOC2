namespace CoreApi.Domain;

public class Rol
{
    public Guid RolID { get; set; } = Guid.NewGuid();
    public string Naam { get; set; } = string.Empty;
    public string Beschrijving { get; set; } = string.Empty;

    public ICollection<Gebruiker> Gebruikers { get; set; } = new List<Gebruiker>();
    public ICollection<Recht> Rechten { get; set; } = new List<Recht>();
}
