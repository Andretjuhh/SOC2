using CoreApi.Domain;
using CoreApi.Features.Auth.GetMe;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace CoreApi.Tests;

public sealed class VerifyMfaTests : MfaTestBase
{
    // ---------- Acceptatiecriterium 2: juiste code => ingelogd, rol en klanttoegang ----------

    [Fact]
    public async Task JuisteCode_GeeftSessiecookie()
    {
        var (_, key, id) = await Setup();
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
        Assert.True(HeeftSessiecookie);
        var cookie = http.Response.Headers.SetCookie.ToString();
        Assert.Contains("SOC2.Session", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task JuisteCode_GeeftRolEnRechten()
    {
        var (user, key, id) = await Setup();
        var ok = Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
        Assert.Equal(user.Id, ok.Value!.Id);
        Assert.Equal("SOC-analist", ok.Value.Rol);
        Assert.Contains("alerts.lezen", ok.Value.Rechten);
    }

    [Fact]
    public async Task JuisteCode_MetActieveKlant_GeeftKlanttoegang()
    {
        var klant = new Klant { naam = "Acme", isActief = true };
        var (_, key, id) = await Setup(klant: klant);
        var ok = Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
        Assert.True(ok.Value!.HeeftKlanttoegang);
        Assert.Equal(klant.KlantID, ok.Value.KlantId);
        Assert.Null(ok.Value.Melding);
    }

    [Fact]
    public async Task JuisteCode_ZonderKlant_GeeftMeldingEnGeenKlanttoegang()
    {
        var (_, key, id) = await Setup();
        var ok = Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
        Assert.False(ok.Value!.HeeftKlanttoegang);
        Assert.NotNull(ok.Value.Melding);
    }

    [Fact]
    public async Task JuisteCode_MetInactieveKlant_GeeftGeenKlanttoegang()
    {
        var (_, key, id) = await Setup(klant: new Klant { naam = "Acme", isActief = false });
        var ok = Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
        Assert.False(ok.Value!.HeeftKlanttoegang);
    }

    [Fact]
    public async Task JuisteCode_MetSpatiesEromheen_WordtGeaccepteerd()
    {
        var (_, key, id) = await Setup();
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, $"  {Totp(key)} "));
    }

    [Fact]
    public async Task CodeVanVorigeTijdstap_WordtNogGeaccepteerd_VoorKlokverschil()
    {
        var (_, key, id) = await Setup();
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key, stepOffset: -1)));
    }

    [Fact]
    public async Task JuisteCode_ResetTeller_VanMisluktePogingen()
    {
        var (user, key, id) = await Setup();
        await Verify(id, FouteCode(key));
        await Verify(id, FouteCode(key));
        Assert.Equal(2, await users.GetAccessFailedCountAsync(user));
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
        Assert.Equal(0, await users.GetAccessFailedCountAsync(user));
    }

    // ---------- Acceptatiecriterium 3 / U2a: onjuiste of verlopen code => geen toegang ----------

    [Fact]
    public async Task OnjuisteCode_GeeftDuidelijkeFoutmelding_ZonderSessie()
    {
        var (_, key, id) = await Setup();
        var problem = Probleem(await Verify(id, FouteCode(key)));
        Assert.Equal(401, problem.StatusCode);
        Assert.Equal("De ingevoerde verificatiecode is onjuist of verlopen. Probeer het opnieuw.",
            problem.ProblemDetails.Title);
        Assert.False(IsChallengeVerlopen(problem)); // gebruiker blijft op het MFA-scherm
        Assert.False(HeeftSessiecookie);
    }

    [Fact]
    public async Task OnjuisteCode_ChallengeBlijftBruikbaar_VoorEenNieuwePoging()
    {
        var (_, key, id) = await Setup();
        Assert.Equal(401, Probleem(await Verify(id, FouteCode(key))).StatusCode);
        Assert.NotNull(challenges.Find(id));
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
    }

    [Fact]
    public async Task VerlopenCode_WordtGeweigerd()
    {
        var (_, key, id) = await Setup();
        var problem = Probleem(await Verify(id, Totp(key, stepOffset: -10))); // 5 minuten oud
        Assert.Equal(401, problem.StatusCode);
        Assert.False(HeeftSessiecookie);
    }

    [Fact]
    public async Task ToekomstigeCode_WordtGeweigerd()
    {
        var (_, key, id) = await Setup();
        Assert.Equal(401, Probleem(await Verify(id, Totp(key, stepOffset: 10))).StatusCode);
        Assert.False(HeeftSessiecookie);
    }

    [Fact]
    public async Task CodeVanAnderAccount_WordtGeweigerd()
    {
        var (_, _, id) = await Setup("analist");
        var (_, andereKey, _) = await Setup("collega");
        Assert.Equal(401, Probleem(await Verify(id, Totp(andereKey))).StatusCode);
        Assert.False(HeeftSessiecookie);
    }

    [Fact]
    public async Task AccountZonderAuthenticator_KanNooitVerifieren()
    {
        var (user, _, id) = await Setup(metAuthenticator: false);
        Assert.Equal(401, Probleem(await Verify(id, "123456")).StatusCode);
        Assert.False(HeeftSessiecookie);
        Assert.Equal(1, await users.GetAccessFailedCountAsync(user));
    }

    [Fact]
    public async Task OnjuisteCode_VerhoogtDeFoutenteller_VanHetAccount()
    {
        var (user, key, id) = await Setup();
        await Verify(id, FouteCode(key));
        Assert.Equal(1, await users.GetAccessFailedCountAsync(user));
    }

    // ---------- Brute-force- en replay-bescherming ----------

    [Fact]
    public async Task VierFouteCodes_LatenDeChallengeNogToe()
    {
        var (_, key, id) = await Setup();
        for (var i = 0; i < 4; i++) Assert.Equal(401, Probleem(await Verify(id, FouteCode(key))).StatusCode);
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
    }

    [Fact]
    public async Task VijfFouteCodes_TrekkenDeChallengeInEnBlokkerenHetAccount()
    {
        var (user, key, id) = await Setup();
        for (var i = 0; i < 5; i++) Assert.Equal(401, Probleem(await Verify(id, FouteCode(key))).StatusCode);

        var daarna = Probleem(await Verify(id, Totp(key))); // zelfs de juiste code werkt niet meer
        Assert.Equal(401, daarna.StatusCode);
        Assert.True(IsChallengeVerlopen(daarna));
        Assert.True(await users.IsLockedOutAsync(user));
        Assert.False(HeeftSessiecookie);
    }

    [Fact]
    public async Task GeblokkeerdAccount_KrijgtGeenSessie_EnChallengeWordtVerbruikt()
    {
        var (user, key, id) = await Setup();
        await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(5));
        var problem = Probleem(await Verify(id, Totp(key)));
        Assert.True(IsChallengeVerlopen(problem));
        Assert.Null(challenges.Find(id));
        Assert.False(HeeftSessiecookie);
    }

    [Fact]
    public async Task InactiefAccount_KrijgtGeenSessie()
    {
        var (_, key, id) = await Setup(actief: false);
        var problem = Probleem(await Verify(id, Totp(key)));
        Assert.True(IsChallengeVerlopen(problem));
        Assert.False(HeeftSessiecookie);
    }

    [Fact]
    public async Task Challenge_IsEenmaligTeGebruiken()
    {
        var (_, key, id) = await Setup();
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, Totp(key)));
        var tweede = Probleem(await Verify(id, Totp(key)));
        Assert.True(IsChallengeVerlopen(tweede));
    }

    [Fact]
    public async Task Code_KanNietTweeMaalWordenGebruikt_ReplayBescherming()
    {
        var (user, key, id) = await Setup();
        var code = Totp(key);
        Assert.IsType<Ok<GetMeResponse>>(await Verify(id, code));

        var nieuweChallenge = challenges.Create(user.Id).Id; // bijv. een tweede login met de afgeluisterde code
        Assert.Equal(401, Probleem(await Verify(nieuweChallenge, code)).StatusCode);
    }

    [Fact]
    public async Task OnbekendeChallenge_WordtGeweigerd_EnVraagOmOpnieuwInloggen()
    {
        var problem = Probleem(await Verify(new string('A', 64), "123456"));
        Assert.Equal(401, problem.StatusCode);
        Assert.True(IsChallengeVerlopen(problem));
        Assert.False(HeeftSessiecookie);
    }

    // ---------- Invoervalidatie ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    [InlineData("12 456")]
    [InlineData("12345a")]
    public async Task OngeldigCodeformaat_GeeftValidatiefout_ZonderDePoging_TeTellen(string code)
    {
        var (user, _, id) = await Setup();
        var problem = Probleem(await Verify(id, code));
        Assert.Equal(400, problem.StatusCode);
        Assert.False(HeeftSessiecookie);
        Assert.NotNull(challenges.Find(id));
        Assert.Equal(0, await users.GetAccessFailedCountAsync(user));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LeegChallengeId_GeeftValidatiefout(string challengeId)
    {
        Assert.Equal(400, Probleem(await Verify(challengeId, "123456")).StatusCode);
    }
}
