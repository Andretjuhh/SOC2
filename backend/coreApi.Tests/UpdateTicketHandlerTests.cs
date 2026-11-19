using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using CoreApi.Features.Tickets.UpdateTicket;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreApi.Tests;

public sealed class UpdateTicketHandlerTests
{
    [Fact]
    public async Task UpdateZonderVersie_GeeftValidatiefout()
    {
        var options = new DbContextOptionsBuilder<SOC2DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new SOC2DBContext(options);
        var klant = new Klant { isActief = true };
        var rol = new Rol
        {
            Naam = "analist",
            Rechten = { new Recht { Naam = TicketToegang.RechtSchrijven } }
        };
        var gebruiker = new Gebruiker
        {
            UserName = "analist",
            NormalizedUserName = "ANALIST",
            Naam = "Analist",
            IsActief = true,
            Klant = klant,
            KlantID = klant.KlantID,
            Rol = rol,
            RolID = rol.RolID
        };
        var ticket = new Ticket
        {
            Titel = "Testticket",
            Beschrijving = "Testbeschrijving",
            Klant = klant,
            KlantID = klant.KlantID,
            AangemaaktDoor = gebruiker.Id,
            Versie = [1]
        };
        db.Add(ticket);
        db.Add(gebruiker);
        await db.SaveChangesAsync();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, gebruiker.Id.ToString())], "Test"));
        var result = await UpdateTicketHandler.Handle(
            ticket.TicketID,
            new UpdateTicketRequest(null, null, null, null, null, null),
            principal,
            db,
            CancellationToken.None);

        Assert.Equal(400, ((IStatusCodeHttpResult)result).StatusCode);
    }
}
