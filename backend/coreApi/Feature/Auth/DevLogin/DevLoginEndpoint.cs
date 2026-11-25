using CoreApi.Domain;
using CoreApi.Data;
using Microsoft.AspNetCore.Identity;

namespace CoreApi.Features.Auth.DevLogin;

// Alleen voor de drie seedaccounts in Development: testsessie zonder wachtwoord en MFA.
public static class DevLoginEndpoint
{
    public static RouteHandlerBuilder MapDevLoginEndpoint(this RouteGroupBuilder group) => group
        .MapPost("/login/{gebruikersnaam}", Handle).WithName("DevLogin");

    private static async Task<IResult> Handle(string gebruikersnaam,
        UserManager<Gebruiker> users, SignInManager<Gebruiker> signIn)
    {
        var gebruiker = await users.FindByNameAsync(gebruikersnaam.Trim());
        if (gebruiker is null || !gebruiker.IsActief || !AccountSeeder.IsTestAccount(gebruiker)
            || await users.IsLockedOutAsync(gebruiker))
            return Results.NotFound(new { melding = "Gebruiker niet gevonden of niet actief." });

        await signIn.SignInAsync(gebruiker, isPersistent: false);
        return Results.Ok(new { melding = $"Ingelogd als {gebruiker.UserName}." });
    }
}
