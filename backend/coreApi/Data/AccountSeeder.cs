using System.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Data;

public class AccountSeeder(
    SOC2DBContext db,
    UserManager<Gebruiker> users)
{
    private const string TestWachtwoord = "SOC2 testaccount 2026!";
    private static readonly Guid TestKlantId = Guid.Parse("9f84aed1-c1d4-448a-bf9e-90231e10ad27");

    internal static bool IsTestAccount(Gebruiker gebruiker)
    {
        return gebruiker.KlantID == TestKlantId
            && gebruiker.UserName is "klant" or "analist" or "beheerder";
    }

    public async Task SeedAsync()
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        await MaakTestklant();
        await MaakKlantAccount();
        await MaakAnalistAccount();
        await MaakBeheerderAccount();

        await transaction.CommitAsync();
    }

    private async Task MaakTestklant()
    {
        if (await db.Klanten.AnyAsync(klant => klant.KlantID == TestKlantId))
            return;

        db.Klanten.Add(new Klant
        {
            KlantID = TestKlantId,
            naam = "SOC2 Ontwikkelklant",
            email = "klant@example.test",
            SLA = "Ontwikkeling",
            isActief = true
        });
        await db.SaveChangesAsync();
    }

    private async Task MaakKlantAccount()
    {
        // Hoort eigen klantgegevens en tickets te lezen, geen tickets te wijzigen.
        // Alerts hebben momenteel nog geen aparte rechtencontrole.
        var rol = await ZoekOfMaakRol("Klant", [TicketToegang.RechtLezen]);
        var gebruiker = new Gebruiker
        {
            Naam = "Testklant",
            UserName = "klant",
            Email = "klant@example.test",
            IsActief = true,
            KlantID = TestKlantId,
            RolID = rol.RolID
        };
        await BewaarAccount(gebruiker);
    }

    private async Task MaakAnalistAccount()
    {
        // Hoort alerts en tickets voor toegewezen klanten te behandelen,
        // geen gebruikers of rollen te beheren.
        var rol = await ZoekOfMaakRol("SOC-analist", [TicketToegang.RechtLezen, TicketToegang.RechtSchrijven]);
        var gebruiker = new Gebruiker
        {
            Naam = "Testanalist",
            UserName = "analist",
            Email = "analist@example.test",
            IsActief = true,
            KlantID = TestKlantId,
            RolID = rol.RolID
        };
        await BewaarAccount(gebruiker);
    }

    private async Task MaakBeheerderAccount()
    {
        // Hoort uiteindelijk alle klanten, gebruikers en rollen te beheren.
        // Nu gelden de ticketrechten voor de testklant; globale toegang volgt nog.
        var rol = await ZoekOfMaakRol("Beheerder", [TicketToegang.RechtLezen, TicketToegang.RechtSchrijven]);
        var gebruiker = new Gebruiker
        {
            Naam = "Testbeheerder",
            UserName = "beheerder",
            Email = "beheerder@example.test",
            IsActief = true,
            KlantID = TestKlantId,
            RolID = rol.RolID
        };
        await BewaarAccount(gebruiker);
    }

    private async Task<Rol> ZoekOfMaakRol(string naam, string[] rechten)
    {
        var rol = await db.Rollen.Include(rol => rol.Rechten)
            .SingleOrDefaultAsync(rol => rol.Naam == naam);

        if (rol is null)
        {
            rol = new Rol { Naam = naam };
            db.Rollen.Add(rol);
        }

        foreach (var rechtNaam in rechten)
        {
            if (rol.Rechten.Any(recht => recht.Naam.Equals(rechtNaam, StringComparison.OrdinalIgnoreCase)))
                continue;
            var recht = await db.Rechten.SingleOrDefaultAsync(recht => recht.Naam == rechtNaam);
            rol.Rechten.Add(recht ?? new Recht { Naam = rechtNaam });
        }
        await db.SaveChangesAsync();
        return rol;
    }

    private async Task BewaarAccount(Gebruiker gebruiker)
    {
        var bestaand = await users.FindByNameAsync(gebruiker.UserName!);
        if (bestaand is not null)
        {
            if (bestaand.RolID != gebruiker.RolID || bestaand.KlantID != gebruiker.KlantID)
                throw new InvalidOperationException($"Account '{gebruiker.UserName}' bestaat met een andere rol of klant.");
            return;
        }

        var result = await users.CreateAsync(gebruiker, TestWachtwoord);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(fout => fout.Description)));
    }
}
