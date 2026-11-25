using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Gebruikers.Beheer;

public sealed record RolToekennenRequest(Guid RolId, Guid? KlantId);

public static class GebruikersBeheerEndpoints
{
    public static void MapGebruikersBeheerEndpoints(this IEndpointRouteBuilder app)
    {
        var groep = app.MapGroup("/api").WithTags("Gebruikersbeheer").RequireAuthorization();
        groep.MapGet("/gebruikers", ListGebruikers);
        groep.MapGet("/rollen", ListRollen);
        groep.MapGet("/klanten", ListKlanten);
        groep.MapPut("/gebruikers/{id:guid}/rol", KenRolToe);
    }

    private static async Task<bool> IsBeheerder(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        return gebruiker is not null && RechtenMatrix.IsBeheerder(gebruiker);
    }

    private static async Task<IResult> ListGebruikers(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        if (!await IsBeheerder(principal, db, ct)) return Results.Forbid();
        return Results.Ok(await db.Gebruikers.AsNoTracking().OrderBy(gebruiker => gebruiker.UserName)
            .Select(gebruiker => new
            {
                gebruiker.Id, gebruiker.Naam, Gebruikersnaam = gebruiker.UserName,
                RolId = gebruiker.RolID, Rol = gebruiker.Rol.Naam, KlantId = gebruiker.KlantID, gebruiker.IsActief
            }).ToListAsync(ct));
    }

    private static async Task<IResult> ListRollen(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        if (!await IsBeheerder(principal, db, ct)) return Results.Forbid();
        string[] standaardrollen = [RechtenMatrix.GeenToegang, RechtenMatrix.Klant, RechtenMatrix.Analist, RechtenMatrix.Beheerder];
        return Results.Ok(await db.Rollen.AsNoTracking().Where(rol => standaardrollen.Contains(rol.Naam))
            .OrderBy(rol => rol.Naam).Select(rol => new { Id = rol.RolID, rol.Naam }).ToListAsync(ct));
    }

    private static async Task<IResult> ListKlanten(ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        if (!await IsBeheerder(principal, db, ct)) return Results.Forbid();
        return Results.Ok(await db.Klanten.AsNoTracking().Where(klant => klant.isActief)
            .OrderBy(klant => klant.naam).Select(klant => new { Id = klant.KlantID, Naam = klant.naam }).ToListAsync(ct));
    }

    private static async Task<IResult> KenRolToe(Guid id, RolToekennenRequest request,
        ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        if (!await IsBeheerder(principal, db, ct)) return Results.Forbid();
        var gebruiker = await db.Gebruikers.SingleOrDefaultAsync(gebruiker => gebruiker.Id == id, ct);
        if (gebruiker is null) return Results.NotFound();
        var rol = await db.Rollen.FindAsync([request.RolId], ct);
        if (rol is null || rol.Naam is not (RechtenMatrix.GeenToegang or RechtenMatrix.Klant or RechtenMatrix.Analist or RechtenMatrix.Beheerder))
            return Results.BadRequest(new { melding = "Kies een standaardrol." });

        if (AccountSeeder.IsTestAccount(gebruiker) &&
            (gebruiker.RolID != rol.RolID || gebruiker.KlantID != request.KlantId))
            return Results.Conflict(new { melding = "De drie standaardtestaccounts behouden hun vaste rol en testklant. Gebruik een ander account voor rolwijzigingen." });

        if (request.KlantId.HasValue && !await db.Klanten.AnyAsync(klant => klant.KlantID == request.KlantId && klant.isActief, ct))
            return Results.BadRequest(new { melding = "Kies een actieve klant." });
        if (rol.Naam is RechtenMatrix.Klant or RechtenMatrix.Analist && request.KlantId is null)
            return Results.BadRequest(new { melding = "Deze rol moet aan een klant gekoppeld zijn." });

        gebruiker.Rol = rol;
        gebruiker.RolID = rol.RolID;
        gebruiker.KlantID = request.KlantId;
        gebruiker.ConcurrencyStamp = Guid.NewGuid().ToString();
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(new { melding = "De gebruiker is intussen gewijzigd. Ververs het overzicht." }); }
        return Results.NoContent();
    }
}
