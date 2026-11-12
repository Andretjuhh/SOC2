using CoreApi.Domain;
using Microsoft.AspNetCore.Identity;

namespace CoreApi.Features.Auth.DevLogin;

// ALLEEN VOOR DEVELOPMENT: inloggen zonder wachtwoord en MFA, zodat we de API kunnen testen.
// Weghalen zodra de echte MFA-login af is.
public static class DevLoginEndpoint
{
    public static RouteHandlerBuilder MapDevLoginEndpoint(this RouteGroupBuilder group) => group
        .MapPost("/login/{gebruikersnaam}", Handle).WithName("DevLogin");

    private static async Task<IResult> Handle(string gebruikersnaam,
        UserManager<Gebruiker> users, SignInManager<Gebruiker> signIn)
    {
        var gebruiker = await users.FindByNameAsync(gebruikersnaam.Trim());
        if (gebruiker is null || !gebruiker.IsActief)
            return Results.NotFound(new { melding = "Gebruiker niet gevonden of niet actief." });

        await signIn.SignInAsync(gebruiker, isPersistent: false);
        return Results.Ok(new { melding = $"Ingelogd als {gebruiker.UserName}." });
    }
}