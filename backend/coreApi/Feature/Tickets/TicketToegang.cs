using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Tickets;

// Hulpmethodes om te controleren wie er is ingelogd en wat die mag (RBAC + klantomgeving)
public static class TicketToegang
{
    public const string RechtLezen = "tickets.lezen";
    public const string RechtSchrijven = "tickets.schrijven";

    // Haalt de ingelogde gebruiker op met klant, rol en rechten. Null = niet ingelogd of niet actief.
    public static async Task<Gebruiker?> HaalGebruikerOp(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return null;

        var gebruiker = await db.Gebruikers
            .Include(x => x.Klant)
            .Include(x => x.Rol).ThenInclude(x => x.Rechten)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

        if (gebruiker is null || !gebruiker.IsActief)
            return null;

        return gebruiker;
    }

    public static bool MagLezen(Gebruiker gebruiker) =>
        HeeftActieveKlant(gebruiker) && (HeeftRecht(gebruiker, RechtLezen) || HeeftRecht(gebruiker, RechtSchrijven));

    public static bool MagSchrijven(Gebruiker gebruiker) =>
        HeeftActieveKlant(gebruiker) && HeeftRecht(gebruiker, RechtSchrijven);

    // UU6b: melding als iemand anders het ticket tussentijds heeft gewijzigd
    public static IResult ConflictMelding() => Results.Problem(statusCode: 409,
        title: "Dit ticket is tussentijds gewijzigd door een andere gebruiker. Ververs de pagina om de meest actuele gegevens te zien.");

    private static bool HeeftActieveKlant(Gebruiker gebruiker) =>
        gebruiker.Klant is { isActief: true };

    private static bool HeeftRecht(Gebruiker gebruiker, string recht) =>
        gebruiker.Rol.Rechten.Any(x => x.Naam.Equals(recht, StringComparison.OrdinalIgnoreCase));
}