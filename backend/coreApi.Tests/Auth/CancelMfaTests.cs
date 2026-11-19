using CoreApi.Features.Auth.CancelMfa;
using CoreApi.Features.Auth.GetMe;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace CoreApi.Tests;

/// <summary>U2b: de gebruiker annuleert de MFA-stap en wordt teruggestuurd naar het inlogscherm.</summary>
public sealed class CancelMfaTests : MfaTestBase
{
    private Task<Microsoft.AspNetCore.Http.IResult> Annuleer(string? id) =>
        CancelMfaHandler.Handle(new CancelMfaRequest(id), http, challenges);

    [Fact]
    public async Task Annuleren_GeeftNoContent_EnTrektDeChallengeIn()
    {
        var (_, _, id) = await Setup();
        Assert.IsType<NoContent>(await Annuleer(id));
        Assert.Null(challenges.Find(id));
    }

    [Fact]
    public async Task NaAnnuleren_WerktEenJuisteCodeNietMeer()
    {
        var (_, key, id) = await Setup();
        await Annuleer(id);
        var problem = Probleem(await Verify(id, Totp(key)));
        Assert.Equal(401, problem.StatusCode);
        Assert.True(IsChallengeVerlopen(problem));
    }

    [Fact]
    public async Task Annuleren_VerwijdertDeSessiecookie()
    {
        var (_, _, id) = await Setup();
        await Annuleer(id);
        var cookie = http.Response.Headers.SetCookie.ToString();
        Assert.Contains("SOC2.Session", cookie);
        Assert.Contains("1970", cookie); // verlopen cookie = verwijderd
    }

    [Fact]
    public async Task Annuleren_RaaktAndereChallengesNiet()
    {
        var (_, _, id) = await Setup("analist");
        var (_, key, ander) = await Setup("collega");
        await Annuleer(id);
        Assert.NotNull(challenges.Find(ander));
        Assert.IsType<Ok<GetMeResponse>>(await Verify(ander, Totp(key)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("bestaat-niet")]
    public async Task Annuleren_MetLeegOfOnbekendId_GeeftGewoonNoContent(string? id) =>
        Assert.IsType<NoContent>(await Annuleer(id));

    [Fact]
    public async Task Annuleren_MetTeLangId_GeeftNoContent_ZonderDeStoreTeRaadplegen() =>
        Assert.IsType<NoContent>(await Annuleer(new string('A', 500)));
}
