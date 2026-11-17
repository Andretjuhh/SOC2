using System.Security.Claims;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using CoreApi.Features.Tickets.CreateTicket;
using CoreApi.Features.Tickets.GetTicket;
using CoreApi.Features.Tickets.ResolveTicket;
using CoreApi.Features.Tickets.UpdateTicket;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreApi.Tests;

public sealed class TicketTests(TicketTestDatabase database) : IClassFixture<TicketTestDatabase>
{
    // ---------- Hulpmethodes ----------

    // Maakt een klant + gebruiker met de opgegeven rechten en geeft de "ingelogde" gebruiker terug
    private async Task<ClaimsPrincipal> MaakGebruiker(params string[] rechten)
    {
        await using var db = database.NieuweContext();
        var naam = "test-" + Guid.NewGuid().ToString("N")[..8];

        var klant = new Klant { naam = "Testklant", email = "klant@test.nl", isActief = true };
        var rol = new Rol { Naam = naam };
        foreach (var recht in rechten)
        {
            var bestaand = await db.Rechten.SingleOrDefaultAsync(x => x.Naam == recht);
            rol.Rechten.Add(bestaand ?? new Recht { Naam = recht });
        }

        var gebruiker = new Gebruiker
        {
            Naam = naam, UserName = naam, NormalizedUserName = naam.ToUpperInvariant(),
            IsActief = true, Klant = klant, Rol = rol
        };
        db.Gebruikers.Add(gebruiker);
        await db.SaveChangesAsync();

        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, gebruiker.Id.ToString())], "Test"));
    }

    private Task<ClaimsPrincipal> MaakAnalist() =>
        MaakGebruiker(TicketToegang.RechtLezen, TicketToegang.RechtSchrijven);

    private async Task<TicketResponse> MaakTicket(ClaimsPrincipal analist)
    {
        await using var db = database.NieuweContext();
        var result = await CreateTicketHandler.Handle(
            new CreateTicketRequest("Verdachte login", "Meerdere mislukte logins",
                TicketPrioriteit.Hoog, TicketCategorie.OngeautoriseerdeToegang),
            analist, db, CancellationToken.None);
        Assert.Equal(201, StatusCode(result));
        return Waarde(result);
    }

    private static int? StatusCode(IResult result) => ((IStatusCodeHttpResult)result).StatusCode;

    private static TicketResponse Waarde(IResult result) =>
        (TicketResponse)((IValueHttpResult)result).Value!;

    private static string? Titel(IResult result) =>
        ((Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult)result).ProblemDetails.Title;

    // ---------- Acceptatiecriterium 1: aanmaken ----------

    [Fact]
    public async Task Aanmaken_GeeftStatusOpenEnUniekTicketnummer()
    {
        var analist = await MaakAnalist();

        var eerste = await MaakTicket(analist);
        var tweede = await MaakTicket(analist);

        Assert.Equal(TicketStatus.Open, eerste.Status);
        Assert.StartsWith("SOC-", eerste.TicketNummer);
        Assert.NotEqual(eerste.TicketNummer, tweede.TicketNummer);
    }

    [Fact]
    public async Task Aanmaken_ZonderTitel_Geeft400()
    {
        var analist = await MaakAnalist();
        await using var db = database.NieuweContext();

        var result = await CreateTicketHandler.Handle(
            new CreateTicketRequest("", "beschrijving", TicketPrioriteit.Laag, TicketCategorie.Overig),
            analist, db, CancellationToken.None);

        Assert.Equal(400, StatusCode(result));
    }

    [Fact]
    public async Task Aanmaken_ZonderSchrijfrecht_Geeft403()
    {
        var lezer = await MaakGebruiker(TicketToegang.RechtLezen);
        await using var db = database.NieuweContext();

        var result = await CreateTicketHandler.Handle(
            new CreateTicketRequest("Titel", "beschrijving", TicketPrioriteit.Laag, TicketCategorie.Overig),
            lezer, db, CancellationToken.None);

        Assert.Equal(403, StatusCode(result));
    }

    [Fact]
    public async Task Ophalen_TicketVanAndereKlant_Geeft404()
    {
        var analistKlantA = await MaakAnalist();
        var analistKlantB = await MaakAnalist();   // MaakGebruiker maakt steeds een nieuwe klant
        var ticket = await MaakTicket(analistKlantA);
        await using var db = database.NieuweContext();

        var result = await GetTicketHandler.Handle(ticket.TicketID, analistKlantB, db, CancellationToken.None);

        Assert.Equal(404, StatusCode(result));
    }

    // ---------- Acceptatiecriterium 2 + UU6a: afronden ----------

    [Fact]
    public async Task Afronden_ZonderOplossing_WordtGeblokkeerdEnStatusBlijftGelijk()
    {
        var analist = await MaakAnalist();
        var ticket = await MaakTicket(analist);

        await using (var db = database.NieuweContext())
        {
            var result = await ResolveTicketHandler.Handle(ticket.TicketID,
                new ResolveTicketRequest("   ", ticket.Versie), analist, db, CancellationToken.None);
            Assert.Equal(400, StatusCode(result));
        }

        await using (var db = database.NieuweContext())
        {
            var opgeslagen = await db.Tickets.SingleAsync(x => x.TicketID == ticket.TicketID);
            Assert.Equal(TicketStatus.Open, opgeslagen.Status);
            Assert.Null(opgeslagen.Oplossing);
        }
    }

    [Fact]
    public async Task Afronden_MetOplossing_ZetStatusOpAfgerondEnKoppeltOplossing()
    {
        var analist = await MaakAnalist();
        var ticket = await MaakTicket(analist);
        await using var db = database.NieuweContext();

        var result = await ResolveTicketHandler.Handle(ticket.TicketID,
            new ResolveTicketRequest("Account geblokkeerd", ticket.Versie), analist, db, CancellationToken.None);

        Assert.Equal(200, StatusCode(result));
        var afgerond = Waarde(result);
        Assert.Equal(TicketStatus.Afgerond, afgerond.Status);
        Assert.Equal("Account geblokkeerd", afgerond.Oplossing);
        Assert.NotNull(afgerond.AfgerondOp);
    }

    [Fact]
    public async Task Bewerken_NaAfronden_Geeft409()
    {
        var analist = await MaakAnalist();
        var ticket = await MaakTicket(analist);

        TicketResponse afgerond;
        await using (var db = database.NieuweContext())
            afgerond = Waarde(await ResolveTicketHandler.Handle(ticket.TicketID,
                new ResolveTicketRequest("Opgelost", ticket.Versie), analist, db, CancellationToken.None));

        await using (var db = database.NieuweContext())
        {
            var result = await UpdateTicketHandler.Handle(ticket.TicketID,
                new UpdateTicketRequest("Nieuwe titel", null, null, null, null, afgerond.Versie),
                analist, db, CancellationToken.None);
            Assert.Equal(409, StatusCode(result));
        }
    }

    // ---------- Acceptatiecriterium 3: historie ----------

    [Fact]
    public async Task VolledigeLevenscyclus_WordtVastgelegdInHistorie()
    {
        var analist = await MaakAnalist();
        var ticket = await MaakTicket(analist);

        TicketResponse inBehandeling;
        await using (var db = database.NieuweContext())
            inBehandeling = Waarde(await UpdateTicketHandler.Handle(ticket.TicketID,
                new UpdateTicketRequest(null, null, null, null, TicketStatus.InBehandeling, ticket.Versie),
                analist, db, CancellationToken.None));

        await using (var db = database.NieuweContext())
            await ResolveTicketHandler.Handle(ticket.TicketID,
                new ResolveTicketRequest("Opgelost", inBehandeling.Versie), analist, db, CancellationToken.None);

        await using (var db = database.NieuweContext())
        {
            var historie = await db.TicketHistorie
                .Where(x => x.TicketID == ticket.TicketID).OrderBy(x => x.TicketHistorieID).ToListAsync();

            Assert.Equal(["Aangemaakt", "Bijgewerkt", "Afgerond"], historie.Select(x => x.Actie));
            Assert.All(historie, regel =>
            {
                Assert.Equal(ticket.AangemaaktDoor, regel.GebruikerID);   // gebruikers-ID
                Assert.NotEqual(default, regel.Tijdstip);                  // tijdstempel
            });
        }
    }

    [Fact]
    public async Task Historie_KanNietWordenGewijzigd_DoorDeCode()
    {
        var analist = await MaakAnalist();
        var ticket = await MaakTicket(analist);
        await using var db = database.NieuweContext();

        var regel = await db.TicketHistorie.FirstAsync(x => x.TicketID == ticket.TicketID);
        regel.Omschrijving = "vervalst";

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Historie_KanNietWordenGewijzigd_DoorDeDatabase()
    {
        var analist = await MaakAnalist();
        var ticket = await MaakTicket(analist);
        await using var db = database.NieuweContext();

        // Direct SQL, dus buiten onze C#-beveiliging om: de trigger moet dit tegenhouden
        await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE TicketHistorie SET Omschrijving = 'vervalst' WHERE TicketID = {ticket.TicketID}"));
        await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM TicketHistorie WHERE TicketID = {ticket.TicketID}"));
    }

    // ---------- UU6b: gelijktijdig bewerken ----------

    [Fact]
    public async Task GelijktijdigBewerken_TweedeOpslagMetOudeVersie_Geeft409()
    {
        var analist = await MaakAnalist();
        var ticket = await MaakTicket(analist);
        var versieDieBeidenZien = ticket.Versie;

        // Analist A slaat als eerste op
        await using (var dbA = database.NieuweContext())
        {
            var resultA = await UpdateTicketHandler.Handle(ticket.TicketID,
                new UpdateTicketRequest(null, null, null, null, TicketStatus.InBehandeling, versieDieBeidenZien),
                analist, dbA, CancellationToken.None);
            Assert.Equal(200, StatusCode(resultA));
        }

        // Analist B probeert daarna op te slaan met de oude versie
        await using (var dbB = database.NieuweContext())
        {
            var resultB = await UpdateTicketHandler.Handle(ticket.TicketID,
                new UpdateTicketRequest(null, null, TicketPrioriteit.Laag, null, null, versieDieBeidenZien),
                analist, dbB, CancellationToken.None);

            Assert.Equal(409, StatusCode(resultB));
            Assert.Equal("Dit ticket is tussentijds gewijzigd door een andere gebruiker. " +
                         "Ververs de pagina om de meest actuele gegevens te zien.", Titel(resultB));
        }

        // De wijziging van B is niet opgeslagen
        await using (var db = database.NieuweContext())
        {
            var opgeslagen = await db.Tickets.SingleAsync(x => x.TicketID == ticket.TicketID);
            Assert.Equal(TicketPrioriteit.Hoog, opgeslagen.Prioriteit);
        }
    }
}