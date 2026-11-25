using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Alerts;

public static class AlertToegang
{
    public static async Task<Gebruiker?> HaalGebruikerOp(
        ClaimsPrincipal principal,
        SOC2DBContext db,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var gebruikerId))
            return null;

        var gebruiker = await db.Gebruikers
            .Include(x => x.Klant)
            .Include(gebruiker => gebruiker.Rol).ThenInclude(rol => rol.Rechten)
            .SingleOrDefaultAsync(x => x.Id == gebruikerId, cancellationToken);

        return gebruiker is { IsActief: true } && RechtenMatrix.HeeftKlanttoegang(gebruiker)
            ? gebruiker
            : null;
    }

    public static bool MagLezen(Gebruiker gebruiker) =>
        RechtenMatrix.HeeftRecht(gebruiker, RechtenMatrix.AlertsLezen);

    public static bool MagSchrijven(Gebruiker gebruiker) =>
        RechtenMatrix.HeeftRecht(gebruiker, RechtenMatrix.AlertsSchrijven);

    public static bool MagClaimen(Gebruiker gebruiker) =>
        RechtenMatrix.HeeftRecht(gebruiker, RechtenMatrix.AlertsClaimen);
}
