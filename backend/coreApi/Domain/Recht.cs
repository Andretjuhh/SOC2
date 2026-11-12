namespace CoreApi.Domain;

public class Recht
{
    public Guid RechtID { get; set; } = Guid.NewGuid();
    public string Naam { get; set; } = string.Empty;
    public string Beschrijving { get; set; } = string.Empty;

    public ICollection<Rol> Rollen { get; set; } = new List<Rol>();
}
