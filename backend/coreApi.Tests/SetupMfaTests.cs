using CoreApi.Features.Auth.GetMe;
using CoreApi.Features.Gebruikers.SetupMfa;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace CoreApi.Tests;

/// <summary>Koppelen van Microsoft Authenticator (dev-endpoint) en de QR-code-URI.</summary>
public sealed class SetupMfaTests : MfaTestBase
{
    private Task<Microsoft.AspNetCore.Http.IResult> Koppel(string naam, string wachtwoord) =>
        SetupMfaEndpoint.Handle(new SetupMfaRequest(naam, wachtwoord), users);

    [Fact]
    public async Task JuisteGegevens_GevenSleutelEnOtpAuthUri()
    {
        var user = await MaakGebruiker(metAuthenticator: false);
        var ok = Assert.IsType<Ok<SetupMfaResponse>>(await Koppel("analist", Password));

        Assert.Equal(await users.GetAuthenticatorKeyAsync(user), ok.Value!.Sleutel);
        Assert.Matches("^[A-Z2-7]+$", ok.Value.Sleutel); // base32
        Assert.True(await users.GetTwoFactorEnabledAsync(user));
    }

    [Fact]
    public async Task OtpAuthUri_HeeftHetFormaatDatMicrosoftAuthenticatorVerwacht()
    {
        await MaakGebruiker(metAuthenticator: false);
        var ok = Assert.IsType<Ok<SetupMfaResponse>>(await Koppel("analist", Password));

        Assert.True(Uri.TryCreate(ok.Value!.OtpAuthUri, UriKind.Absolute, out var uri));
        Assert.Equal("otpauth", uri!.Scheme);
        Assert.Equal("totp", uri.Host);
        Assert.Equal("/SOC2:analist", Uri.UnescapeDataString(uri.AbsolutePath));
        Assert.Contains($"secret={ok.Value.Sleutel}", uri.Query);
        Assert.Contains("issuer=SOC2", uri.Query);
        Assert.Contains("algorithm=SHA1", uri.Query);
        Assert.Contains("digits=6", uri.Query);
        Assert.Contains("period=30", uri.Query);
    }

    [Fact]
    public async Task OtpAuthUri_EscapetSpecialeTekensInDeGebruikersnaam()
    {
        await MaakGebruiker("jan@example.nl", metAuthenticator: false);
        var ok = Assert.IsType<Ok<SetupMfaResponse>>(await Koppel("jan@example.nl", Password));
        Assert.Contains("SOC2:jan%40example.nl", ok.Value!.OtpAuthUri);
    }

    [Fact]
    public async Task GekoppeldeApp_GenereertCodesDieDeLoginAccepteert()
    {
        var user = await MaakGebruiker(metAuthenticator: false);
        var ok = Assert.IsType<Ok<SetupMfaResponse>>(await Koppel("analist", Password));

        var challenge = challenges.Create(user.Id).Id;
        Assert.IsType<Ok<GetMeResponse>>(await Verify(challenge, Totp(ok.Value!.Sleutel)));
    }

    [Fact]
    public async Task OpnieuwKoppelen_VervangtDeSleutel_EnDeOudeAppWerktNietMeer()
    {
        var user = await MaakGebruiker(metAuthenticator: false);
        var eerste = Assert.IsType<Ok<SetupMfaResponse>>(await Koppel("analist", Password)).Value!;
        var tweede = Assert.IsType<Ok<SetupMfaResponse>>(await Koppel("analist", Password)).Value!;
        Assert.NotEqual(eerste.Sleutel, tweede.Sleutel);

        var challenge = challenges.Create(user.Id).Id;
        Assert.Equal(401, Probleem(await Verify(challenge, FouteCode(tweede.Sleutel))).StatusCode);
        Assert.IsType<Ok<GetMeResponse>>(await Verify(challenge, Totp(tweede.Sleutel)));
    }

    [Fact]
    public async Task GebruikersnaamMetSpatiesEnHoofdletters_WordtHerkend()
    {
        await MaakGebruiker(metAuthenticator: false);
        Assert.IsType<Ok<SetupMfaResponse>>(await Koppel("  ANALIST ", Password));
    }

    [Fact]
    public async Task FoutWachtwoord_GeeftGeenSleutel()
    {
        var user = await MaakGebruiker(metAuthenticator: false);
        var problem = Probleem(await Koppel("analist", "verkeerd wachtwoord"));
        Assert.Equal(401, problem.StatusCode);
        Assert.Null(await users.GetAuthenticatorKeyAsync(user));
    }

    [Fact]
    public async Task FoutWachtwoord_LaatEenBestaandeKoppelingOngemoeid()
    {
        var user = await MaakGebruiker();
        var voor = await users.GetAuthenticatorKeyAsync(user);
        Probleem(await Koppel("analist", "verkeerd wachtwoord"));
        Assert.Equal(voor, await users.GetAuthenticatorKeyAsync(user));
    }

    [Fact]
    public async Task OnbekendeGebruiker_GeeftDezelfdeFout()
    {
        var problem = Probleem(await Koppel("bestaat-niet", Password));
        Assert.Equal(401, problem.StatusCode);
        Assert.Equal("Ongeldige inloggegevens.", problem.ProblemDetails.Title);
    }

    [Fact]
    public async Task LegeVelden_GevenGeenCrash_EnEen401()
    {
        var result = await SetupMfaEndpoint.Handle(new SetupMfaRequest(null!, null!), users);
        Assert.Equal(401, Probleem(result).StatusCode);
    }
}
