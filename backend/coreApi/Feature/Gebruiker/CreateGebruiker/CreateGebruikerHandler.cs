using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Gebruikers.CreateGebruiker;

public static class CreateGebruikerHandler
{
    public static async Task<IResult> Handle(CreateGebruikerRequest request,
        SOC2DBContext db, UserManager<Gebruiker> users, CancellationToken ct)
    {
        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["gebruiker"] = [fout] });

        var bestaandeGebruiker = await users.FindByNameAsync(request.Gebruikersnaam.Trim());
        if (bestaandeGebruiker is not null)
            return Results.Conflict(new { melding = "Deze gebruikersnaam bestaat al." });
        if (request.KlantId.HasValue && !await db.Klanten.AnyAsync(x => x.KlantID == request.KlantId, ct))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["klantId"] = ["Klant bestaat niet."] });

        var rol = await db.Rollen.SingleOrDefaultAsync(rol => rol.Naam == RechtenMatrix.GeenToegang, ct);
        if (rol is null) rol = new Rol { Naam = RechtenMatrix.GeenToegang };

        var gebruiker = new Gebruiker
        {
            Naam = request.Naam.Trim(),
            UserName = request.Gebruikersnaam.Trim(),
            Email = request.Email.Trim(),
            IsActief = request.IsActief,
            KlantID = request.KlantId,
            Rol = rol,
            RolID = rol.RolID
        };
        try
        {
            var result = await users.CreateAsync(gebruiker, request.Wachtwoord);
            if (!result.Succeeded)
                return Results.ValidationProblem(result.Errors.GroupBy(x => x.Code)
                    .ToDictionary(x => x.Key, x => x.Select(e => e.Description).ToArray()));
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Results.Conflict(new { melding = "Gebruikersnaam, rol of recht bestaat inmiddels al. Probeer opnieuw." });
        }

        var response = new CreateGebruikerResponse(gebruiker.Id, gebruiker.Naam,
            gebruiker.UserName!, gebruiker.RolID, gebruiker.KlantID, gebruiker.IsActief);
        return Results.Created((string?)null, response);
    }

}
