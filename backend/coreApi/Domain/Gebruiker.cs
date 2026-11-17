using Microsoft.AspNetCore.Identity;

namespace CoreApi.Domain;

public class Gebruiker : IdentityUser<Guid>
{
    public Gebruiker()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public string Naam { get; set; } = string.Empty;

    public bool IsActief { get; set; }

    public Guid? KlantID { get; set; }
    public Klant? Klant { get; set; }

    public Guid RolID { get; set; }
    public Rol Rol { get; set; } = null!;
}
