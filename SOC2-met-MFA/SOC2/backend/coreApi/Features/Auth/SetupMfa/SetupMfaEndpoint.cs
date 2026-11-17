using CoreApi.Domain;
using Microsoft.AspNetCore.Identity;

namespace CoreApi.Features.Gebruikers.SetupMfa;

public sealed record SetupMfaRequest(string Gebruikersnaam, string Wachtwoord);
public sealed record SetupMfaResponse(string Sleutel, string OtpAuthUri);

public static class SetupMfaEndpoint
{
    public static RouteHandlerBuilder MapSetupMfaEndpoint(this RouteGroupBuilder group) => group
        .MapPost("/mfa-setup", Handle).WithName("SetupMfa")
        .Produces<SetupMfaResponse>().Produces(401);

    // Alleen Development: koppelt een (nieuwe) authenticator-sleutel aan een bestaand account.
    public static async Task<IResult> Handle(SetupMfaRequest request, UserManager<Gebruiker> users)
    {
        var gebruiker = await users.FindByNameAsync(request.Gebruikersnaam?.Trim() ?? "");
        if (gebruiker is null || !await users.CheckPasswordAsync(gebruiker, request.Wachtwoord ?? ""))
            return Results.Problem(statusCode: 401, title: "Ongeldige inloggegevens.");

        await users.ResetAuthenticatorKeyAsync(gebruiker);
        var sleutel = (await users.GetAuthenticatorKeyAsync(gebruiker))!;
        await users.SetTwoFactorEnabledAsync(gebruiker, true);

        // Standaard TOTP-URI; Microsoft Authenticator leest label, secret en issuer uit de QR-code.
        const string issuer = "SOC2";
        var label = Uri.EscapeDataString(issuer) + ":" + Uri.EscapeDataString(gebruiker.UserName!);
        var uri = $"otpauth://totp/{label}?secret={sleutel}&issuer={Uri.EscapeDataString(issuer)}"
                  + "&algorithm=SHA1&digits=6&period=30";
        return Results.Ok(new SetupMfaResponse(sleutel, uri));
    }
}
