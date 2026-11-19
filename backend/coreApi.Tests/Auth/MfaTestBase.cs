using System.Security.Cryptography;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Auth;
using CoreApi.Features.Auth.VerifyMfa;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreApi.Tests;

/// <summary>Gedeelde opzet voor alle MFA-tests: InMemory-database, Identity en een eigen TOTP-generator.</summary>
public abstract class MfaTestBase : IDisposable
{
    protected const string Password = "een lange veilige testzin";

    private readonly ServiceProvider provider;
    private readonly IServiceScope scope;
    protected readonly DefaultHttpContext http = new();
    protected readonly UserManager<Gebruiker> users;
    protected readonly SignInManager<Gebruiker> signIn;
    protected readonly MfaChallengeStore challenges;
    protected readonly SOC2DBContext db;

    protected MfaTestBase()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<SOC2DBContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddSOC2Identity(development: true);
        services.AddMemoryCache();
        services.AddSingleton<MfaChallengeStore>();
        provider = services.BuildServiceProvider();
        scope = provider.CreateScope();
        http.RequestServices = scope.ServiceProvider;
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext = http;
        db = scope.ServiceProvider.GetRequiredService<SOC2DBContext>();
        users = scope.ServiceProvider.GetRequiredService<UserManager<Gebruiker>>();
        signIn = scope.ServiceProvider.GetRequiredService<SignInManager<Gebruiker>>();
        challenges = scope.ServiceProvider.GetRequiredService<MfaChallengeStore>();
    }

    protected async Task<Gebruiker> MaakGebruiker(string naam = "analist", bool actief = true,
        bool metAuthenticator = true, Klant? klant = null)
    {
        var user = new Gebruiker
        {
            UserName = naam, Email = naam + "@example.test", Naam = naam, IsActief = actief,
            Klant = klant, KlantID = klant?.KlantID,
            Rol = new Rol { Naam = "SOC-analist", Rechten = { new Recht { Naam = "alerts.lezen" } } }
        };
        user.RolID = user.Rol.RolID;
        var result = await users.CreateAsync(user, Password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(x => x.Description)));
        if (metAuthenticator) await users.ResetAuthenticatorKeyAsync(user);
        return user;
    }

    /// <summary>Maakt een gebruiker met authenticator-sleutel en een openstaande MFA-challenge.</summary>
    protected async Task<(Gebruiker User, string Key, string ChallengeId)> Setup(string naam = "analist",
        bool actief = true, bool metAuthenticator = true, Klant? klant = null)
    {
        var user = await MaakGebruiker(naam, actief, metAuthenticator, klant);
        var key = await users.GetAuthenticatorKeyAsync(user) ?? string.Empty;
        return (user, key, challenges.Create(user.Id).Id);
    }

    protected Task<IResult> Verify(string challengeId, string code) =>
        VerifyMfaHandler.Handle(new(challengeId, code), users, signIn, db, challenges, default);

    protected bool HeeftSessiecookie => http.Response.Headers.SetCookie.Count > 0;

    protected static ProblemHttpResult Probleem(IResult result) => Assert.IsType<ProblemHttpResult>(result);

    protected static bool IsChallengeVerlopen(ProblemHttpResult problem) =>
        problem.ProblemDetails.Extensions.TryGetValue("challengeVerlopen", out var waarde) && waarde is true;

    // --- minimale RFC 6238 (HMAC-SHA1, 30 s, 6 cijfers): dezelfde als Microsoft Authenticator gebruikt ---
    protected static string Totp(string base32, long stepOffset = 0)
    {
        var key = Base32(base32);
        var step = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30 + stepOffset;
        var msg = BitConverter.GetBytes(step);
        if (BitConverter.IsLittleEndian) Array.Reverse(msg);
        var hash = HMACSHA1.HashData(key, msg);
        var o = hash[^1] & 0x0F;
        var bin = ((hash[o] & 0x7F) << 24) | (hash[o + 1] << 16) | (hash[o + 2] << 8) | hash[o + 3];
        return (bin % 1_000_000).ToString("D6");
    }

    /// <summary>Een 6-cijferige code die in geen enkele nabije tijdstap geldig is.</summary>
    protected static string FouteCode(string base32)
    {
        var geldig = Enumerable.Range(-4, 9).Select(o => Totp(base32, o)).ToHashSet();
        for (var n = 0; ; n++)
        {
            var kandidaat = n.ToString("D6");
            if (!geldig.Contains(kandidaat)) return kandidaat;
        }
    }

    private static byte[] Base32(string s)
    {
        const string alfabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bytes = new List<byte>();
        int buffer = 0, bits = 0;
        foreach (var c in s.ToUpperInvariant().TrimEnd('='))
        {
            buffer = (buffer << 5) | alfabet.IndexOf(c);
            bits += 5;
            if (bits >= 8) { bits -= 8; bytes.Add((byte)(buffer >> bits)); buffer &= (1 << bits) - 1; }
        }
        return bytes.ToArray();
    }

    public void Dispose()
    {
        scope.Dispose();
        provider.Dispose();
        GC.SuppressFinalize(this);
    }
}
