using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Auth;
using CoreApi.Features.Auth.GetMe;
using CoreApi.Features.Auth.Login;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace CoreApi.Tests;

public sealed class LoginTests : IDisposable
{
    private const string Password = "een lange veilige testzin";
    private readonly ServiceProvider provider;
    private readonly IServiceScope scope;
    private readonly DefaultHttpContext http = new();
    private readonly UserManager<Gebruiker> users;
    private readonly SignInManager<Gebruiker> signIn;
    private readonly MfaChallengeStore challenges;
    private readonly SOC2DBContext db;

    public LoginTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<SOC2DBContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
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

    private async Task<Gebruiker> AddUser(bool active = true)
    {
        var user = new Gebruiker { UserName = "analist", Naam = "Analist", IsActief = active,
            Rol = new Rol { Naam = "SOC-analist" } };
        user.RolID = user.Rol.RolID;
        var result = await users.CreateAsync(user, Password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(x => x.Description)));
        return user;
    }

    [Fact]
    public async Task Login_GeldigAccount_GeeftAlleenMfaChallengeEnGeenSessie()
    {
        var user = await AddUser();
        var result = await LoginHandler.Handle(new(" ANALIST ", Password), users, signIn, challenges);
        var accepted = Assert.IsType<Accepted<LoginResponse>>(result);
        Assert.True(accepted.Value!.MfaVereist);
        Assert.Equal(user.Id, challenges.Find(accepted.Value.ChallengeId));
        Assert.Equal(0, http.Response.Headers.SetCookie.Count);
        Assert.False(http.User.Identity?.IsAuthenticated ?? false);
    }

    [Theory]
    [InlineData("onbekend", Password, true)]
    [InlineData("analist", "verkeerd wachtwoord", true)]
    [InlineData("analist", Password, false)]
    public async Task Login_OngeldigeOfInactieveGebruiker_GeeftDezelfdeFout(string name, string password, bool active)
    {
        await AddUser(active);
        var result = await LoginHandler.Handle(new(name, password), users, signIn, challenges);
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(401, problem.StatusCode);
        Assert.Equal("Ongeldige inloggegevens.", problem.ProblemDetails.Title);
    }

    [Fact]
    public async Task Login_VijfFoutePogingen_BlokkeertOokHetJuisteWachtwoord()
    {
        var user = await AddUser();
        for (var attempt = 0; attempt < 5; attempt++)
            await LoginHandler.Handle(new("analist", "verkeerd"), users, signIn, challenges);
        Assert.True(await users.IsLockedOutAsync(user));
        var result = await LoginHandler.Handle(new("analist", Password), users, signIn, challenges);
        Assert.Equal(401, Assert.IsType<ProblemHttpResult>(result).StatusCode);
    }

    [Fact]
    public async Task Login_VerstrekenBlokkade_StaatWachtwoordcontroleWeerToe()
    {
        var user = await AddUser();
        await users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(-1));
        var result = await LoginHandler.Handle(new("analist", Password), users, signIn, challenges);
        Assert.IsType<Accepted<LoginResponse>>(result);
    }

    [Fact]
    public async Task GetMe_GeenKlantkoppeling_GeeftEenMeldingEnGeenKlanttoegang()
    {
        var user = await AddUser();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "test"));
        var result = await GetMeHandler.Handle(principal, db, default);
        var response = Assert.IsType<Ok<GetMeResponse>>(result).Value!;
        Assert.False(response.HeeftKlanttoegang);
        Assert.NotNull(response.Melding);
    }

    [Fact]
    public async Task GetMe_InactiefAccount_WeigertEenBestaandeIdentiteit()
    {
        var user = await AddUser(active: false);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "test"));
        Assert.IsType<UnauthorizedHttpResult>(await GetMeHandler.Handle(principal, db, default));
    }

    public void Dispose()
    {
        scope.Dispose();
        provider.Dispose();
    }
}
