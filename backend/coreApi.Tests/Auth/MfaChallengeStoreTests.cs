using CoreApi.Features.Auth;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CoreApi.Tests;

public sealed class MfaChallengeStoreTests : IDisposable
{
    private readonly MemoryCache cache = new(new MemoryCacheOptions());
    private readonly MfaChallengeStore store;

    public MfaChallengeStoreTests() => store = new MfaChallengeStore(cache);

    [Fact]
    public void Create_BewaartDeGebruiker_EnIsVijfMinutenGeldig()
    {
        var gebruiker = Guid.NewGuid();
        var (id, verlooptOp) = store.Create(gebruiker);
        Assert.Equal(gebruiker, store.Find(id));
        var resterend = verlooptOp - DateTimeOffset.UtcNow;
        Assert.InRange(resterend.TotalSeconds, 290, 305);
    }

    [Fact]
    public void Create_GeeftUniekeIdsVan64HexTekens()
    {
        var ids = Enumerable.Range(0, 50).Select(_ => store.Create(Guid.NewGuid()).Id).ToList();
        Assert.Equal(50, ids.Distinct().Count());
        Assert.All(ids, id => Assert.Matches("^[0-9A-F]{64}$", id));
    }

    [Fact]
    public void Find_OnbekendId_GeeftNull() => Assert.Null(store.Find("onbekend"));

    [Fact]
    public void Consume_GeeftDeGebruikerEenmalig()
    {
        var gebruiker = Guid.NewGuid();
        var (id, _) = store.Create(gebruiker);
        Assert.Equal(gebruiker, store.Consume(id));
        Assert.Null(store.Consume(id));
        Assert.Null(store.Find(id));
    }

    [Fact]
    public void RegistreerMislukt_VierKeerBlijftGeldig_VijfdeKeerIntrekken()
    {
        var (id, _) = store.Create(Guid.NewGuid());
        for (var i = 0; i < 4; i++) store.RegistreerMislukt(id);
        Assert.NotNull(store.Find(id));
        store.RegistreerMislukt(id);
        Assert.Null(store.Find(id));
    }

    [Fact]
    public void RegistreerMislukt_TeltPerChallenge()
    {
        var (a, _) = store.Create(Guid.NewGuid());
        var (b, _) = store.Create(Guid.NewGuid());
        for (var i = 0; i < 5; i++) store.RegistreerMislukt(a);
        Assert.Null(store.Find(a));
        Assert.NotNull(store.Find(b));
    }

    [Fact]
    public void Consume_ZetDeFoutenTellerTerug()
    {
        var gebruiker = Guid.NewGuid();
        var (oud, _) = store.Create(gebruiker);
        for (var i = 0; i < 3; i++) store.RegistreerMislukt(oud);
        store.Consume(oud);
        Assert.Null(store.Find(oud));
    }

    [Fact]
    public void MarkeerCodeAlsGebruikt_WeigertHergebruikVanDezelfdeCode()
    {
        var gebruiker = Guid.NewGuid();
        Assert.True(store.MarkeerCodeAlsGebruikt(gebruiker, "123456"));
        Assert.False(store.MarkeerCodeAlsGebruikt(gebruiker, "123456"));
    }

    [Fact]
    public void MarkeerCodeAlsGebruikt_IsPerGebruikerEnPerCode()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        Assert.True(store.MarkeerCodeAlsGebruikt(a, "123456"));
        Assert.True(store.MarkeerCodeAlsGebruikt(a, "654321"));
        Assert.True(store.MarkeerCodeAlsGebruikt(b, "123456"));
    }

    [Fact]
    public void DummyHash_IsGevuld() => Assert.False(string.IsNullOrWhiteSpace(store.DummyHash));

    public void Dispose() => cache.Dispose();
}
