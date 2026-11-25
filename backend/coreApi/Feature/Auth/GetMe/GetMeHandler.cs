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
        return Results.Ok(GetMeResponse.Van(user));
    }
}
