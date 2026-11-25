using CoreApi.Domain;
using CoreApi.Features.Alerts;
using CoreApi.Features.Alerts.CrudAlert.ClaimAlert;
using CoreApi.Features.Auth.GetMe;
using CoreApi.Features.Gebruikers.CreateGebruiker;
using CoreApi.Features.Tickets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Xunit;

namespace CoreApi.Tests;

public sealed class RechtenTests : MfaTestBase
{
    [Fact]
    public async Task NieuwAccount_StartZonderRechtenEnKanGeenAlertClaimen()
    {
        var request = new CreateGebruikerRequest("Nieuw account", "nieuw", "nieuw@example.test", Password);
        var result = await CreateGebruikerHandler.Handle(request, db, users, default);
        Assert.Equal(201, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        var gebruiker = (await users.FindByNameAsync("nieuw"))!;
        Assert.Equal(RechtenMatrix.GeenToegang, gebruiker.Rol.Naam);
        Assert.Empty(gebruiker.Rol.Rechten);
        Assert.False(GetMeResponse.Van(gebruiker).MagDashboard);
        Assert.False(TicketToegang.MagLezen(gebruiker));
        Assert.False(AlertToegang.MagLezen(gebruiker));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, gebruiker.Id.ToString())], "test"));
        Assert.IsType<ForbidHttpResult>(await ClaimAlertHandler.Handle(1, principal, db, default));
    }

    [Theory]
    [InlineData("Geen toegang", false, false, false)]
    [InlineData("Klant", true, false, false)]
    [InlineData("SOC-analist", true, true, false)]
    [InlineData("Beheerder", true, true, true)]
    public void Matrix_ControleertLezenSchrijvenEnBeheer(string rolNaam, bool lezen, bool schrijven, bool beheer)
    {
        var gebruiker = new Gebruiker
        {
            IsActief = true,
            Klant = new Klant { isActief = true },
            Rol = new Rol
            {
                Naam = rolNaam,
                Rechten = RechtenMatrix.VoorRol(rolNaam).Select(naam => new Recht { Naam = naam }).ToList()
            }
        };

        Assert.Equal(lezen, TicketToegang.MagLezen(gebruiker));
        Assert.Equal(schrijven, TicketToegang.MagSchrijven(gebruiker));
        Assert.Equal(lezen, AlertToegang.MagLezen(gebruiker));
        Assert.Equal(schrijven, AlertToegang.MagClaimen(gebruiker));
        Assert.Equal(beheer, RechtenMatrix.IsBeheerder(gebruiker));
        Assert.Equal(lezen, GetMeResponse.Van(gebruiker).MagDashboard);
    }

    [Fact]
    public void AlleenBeheerder_KanZonderKlantkoppelingWerken()
    {
        foreach (var rolNaam in new[] { RechtenMatrix.Klant, RechtenMatrix.Analist, RechtenMatrix.Beheerder })
        {
            var gebruiker = new Gebruiker
            {
                IsActief = true,
                Rol = new Rol { Naam = rolNaam, Rechten = RechtenMatrix.VoorRol(rolNaam).Select(naam => new Recht { Naam = naam }).ToList() }
            };
            Assert.Equal(rolNaam == RechtenMatrix.Beheerder, TicketToegang.MagLezen(gebruiker));
        }
    }
}
