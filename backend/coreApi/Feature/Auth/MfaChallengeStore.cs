using System.Security.Cryptography;
using CoreApi.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace CoreApi.Features.Auth;

public sealed class MfaChallengeStore(IMemoryCache cache)
{
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
            return userId;
        }
    }
}
