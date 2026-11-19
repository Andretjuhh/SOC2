using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Auth.GetMe;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Auth.VerifyMfa;

public static class VerifyMfaHandler
{
    public const string OnjuisteCode = "De ingevoerde verificatiecode is onjuist of verlopen. Probeer het opnieuw.";
    public const string SessieVerlopen = "Je verificatiesessie is verlopen. Log opnieuw in.";

    public static async Task<IResult> Handle(VerifyMfaRequest request, UserManager<Gebruiker> users,
        SignInManager<Gebruiker> signIn, SOC2DBContext db, MfaChallengeStore challenges, CancellationToken ct)
    {
        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["mfa"] = [fout] });

        var challengeId = request.ChallengeId;
        var code = request.Code.Trim();

        var userId = challenges.Find(challengeId);
        if (userId is null) return Verlopen();

        var gebruiker = await users.FindByIdAsync(userId.Value.ToString());
        if (gebruiker is null || !gebruiker.IsActief || await users.IsLockedOutAsync(gebruiker))
        {
            challenges.Consume(challengeId);
            return Verlopen();
        }

        // Zonder authenticator-sleutel kan er nooit een geldige code zijn.
        var sleutel = await users.GetAuthenticatorKeyAsync(gebruiker);
        var geldig = !string.IsNullOrEmpty(sleutel)
            && await users.VerifyTwoFactorTokenAsync(gebruiker, TokenOptions.DefaultAuthenticatorProvider, code)
            && challenges.MarkeerCodeAlsGebruikt(gebruiker.Id, code);

        if (!geldig)
        {
            await users.AccessFailedAsync(gebruiker);       // telt mee voor de account-lockout
            challenges.RegistreerMislukt(challengeId);       // na 5 fouten vervalt de challenge
            return Results.Problem(statusCode: 401, title: OnjuisteCode);
        }

        // Eenmalig te gebruiken: alleen de eerste succesvolle verificatie krijgt een sessie.
        if (challenges.Consume(challengeId) is null) return Verlopen();

        await users.ResetAccessFailedCountAsync(gebruiker);
        await signIn.SignInAsync(gebruiker, isPersistent: false);

        var user = await db.Gebruikers.AsNoTracking().Include(x => x.Klant)
            .Include(x => x.Rol).ThenInclude(x => x.Rechten)
            .SingleAsync(x => x.Id == gebruiker.Id, ct);
        var toegang = user.Klant is { isActief: true };
        return Results.Ok(new GetMeResponse(user.Id, user.Naam, user.Rol.Naam,
            user.Rol.Rechten.Select(x => x.Naam).Order().ToArray(), user.KlantID, toegang,
            toegang ? null : "Er is geen actieve klanttoegang gekoppeld."));
    }

    private static IResult Verlopen() => Results.Problem(statusCode: 401, title: SessieVerlopen,
        extensions: new Dictionary<string, object?> { ["challengeVerlopen"] = true });
}
