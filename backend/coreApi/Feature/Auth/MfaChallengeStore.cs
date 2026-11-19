using System.Security.Cryptography;
using CoreApi.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace CoreApi.Features.Auth;

public sealed class MfaChallengeStore(IMemoryCache cache)
{
    private const int MaxMisluktePogingen = 5;
    private readonly object gate = new();
    public string DummyHash { get; } = new PasswordHasher<Gebruiker>()
        .HashPassword(new Gebruiker(), Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));

    public (string Id, DateTimeOffset VerlooptOp) Create(Guid gebruikerId)
    {
        var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var expiry = DateTimeOffset.UtcNow.AddMinutes(5);
        cache.Set("mfa:" + id, gebruikerId, expiry);
        return (id, expiry);
    }

    public Guid? Find(string id) => cache.TryGetValue<Guid>("mfa:" + id, out var userId) ? userId : null;

    public Guid? Consume(string id)
    {
        lock (gate)
        {
            var userId = Find(id);
            cache.Remove("mfa:" + id);
            cache.Remove("mfa-fail:" + id);
            return userId;
        }
    }

    /// <summary>Telt een foute code. Na 5 fouten wordt de challenge ingetrokken (opnieuw inloggen nodig).</summary>
    public void RegistreerMislukt(string id)
    {
        lock (gate)
        {
            var aantal = (cache.TryGetValue<int>("mfa-fail:" + id, out var c) ? c : 0) + 1;
            if (aantal >= MaxMisluktePogingen)
            {
                cache.Remove("mfa:" + id);
                cache.Remove("mfa-fail:" + id);
                return;
            }
            cache.Set("mfa-fail:" + id, aantal, TimeSpan.FromMinutes(5));
        }
    }

    /// <summary>Voorkomt hergebruik van dezelfde code binnen het geldigheidsvenster (replay).</summary>
    public bool MarkeerCodeAlsGebruikt(Guid gebruikerId, string code)
    {
        lock (gate)
        {
            var key = $"mfa-used:{gebruikerId}:{code}";
            if (cache.TryGetValue(key, out _)) return false;
            cache.Set(key, true, TimeSpan.FromMinutes(3));
            return true;
        }
    }
}
