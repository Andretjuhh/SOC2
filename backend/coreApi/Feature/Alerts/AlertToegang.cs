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
            .SingleOrDefaultAsync(x => x.Id == gebruikerId, cancellationToken);

        return gebruiker is { IsActief: true, Klant: { isActief: true }, KlantID: not null }
            ? gebruiker
            : null;
    }
}
