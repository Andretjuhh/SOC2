using System.Security.Claims;
using CoreApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Auth.GetMe;

public static class GetMeHandler
{
    public static async Task<IResult> Handle(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Results.Unauthorized();
        var user = await db.Gebruikers.AsNoTracking().Include(x => x.Klant)
            .Include(x => x.Rol).ThenInclude(x => x.Rechten).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (user is null || !user.IsActief) return Results.Unauthorized();
        var toegang = user.Klant is { isActief: true };
        return Results.Ok(new GetMeResponse(user.Id, user.Naam, user.Rol.Naam,
            user.Rol.Rechten.Select(x => x.Naam).Order().ToArray(), user.KlantID, toegang,
            toegang ? null : "Er is geen actieve klanttoegang gekoppeld."));
    }
}
