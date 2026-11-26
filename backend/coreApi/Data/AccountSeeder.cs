using System.Data;
using CoreApi.Domain;
using CoreApi.Features.Tickets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Data;

public class AccountSeeder(SOC2DBContext db, UserManager<Gebruiker> users)
{
    //ieder test accounts wachtwoord is SAME SAME, alleen voor de dev omgevinmg.
    private const string TestWachtwoord = "SOC2-testWW67!-15tekens";
    //ieder test accounts klantID is SAME SAME, alleen voor de dev omgevinmg.
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
        var rol = await ZoekRol(RechtenMatrix.GeenToegang);
        if (rol is null) rol = await MaakRol(RechtenMatrix.GeenToegang);
        await WerkRechtenBij(rol, []);
        await MaakKlantAccount();
        await MaakAnalistAccount();
        await MaakBeheerderAccount();

        await transaction.CommitAsync();
    }

    private async Task MaakTestklant()
    {
        //Dit is het bedrijgfaccount dat dus een bedrijf moet gerbuiken
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
        //Dit is dan een randomn ass gebruiker van dus zo'n bedrijf..
        // hoort enkel en allen eigen klantgegevens en tickets te lezen, geen tickets te wijzigen.
        // Mag alerts bekijken, maar geen alerts claimen of aanmaken.
        var rol = await ZoekRol(RechtenMatrix.Klant);
        if (rol is null) rol = await MaakRol(RechtenMatrix.Klant);
        await WerkRechtenBij(rol, RechtenMatrix.VoorRol(RechtenMatrix.Klant));
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
        // hoort alerts en tickets voor toegewezen klanten te behandelen,
        // zou geen gebruikers of rollen mogen beheren.
        var rol = await ZoekRol(RechtenMatrix.Analist);
        if (rol is null) rol = await MaakRol(RechtenMatrix.Analist);
        await WerkRechtenBij(rol, RechtenMatrix.VoorRol(RechtenMatrix.Analist));
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
        // Hoort uiteindelijk denk ik dan alle klanten, gebruikers en rollen te beheren.
        // Heeft klantoverschrijdende toegang en mag rollen aan gebruikers toekennen.
        var rol = await ZoekRol(RechtenMatrix.Beheerder);
        if (rol is null) rol = await MaakRol(RechtenMatrix.Beheerder);
        await WerkRechtenBij(rol, RechtenMatrix.VoorRol(RechtenMatrix.Beheerder));
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

    private async Task<Rol?> ZoekRol(string naam)
    {
        return await db.Rollen.Include(rol => rol.Rechten)
            .SingleOrDefaultAsync(rol => rol.Naam == naam);
    }

    private async Task<Rol> MaakRol(string naam)
    {
        var rol = new Rol { Naam = naam };
        db.Rollen.Add(rol);
        await db.SaveChangesAsync();
        return rol;
    }

    private async Task WerkRechtenBij(Rol rol, string[] rechten)
    {
        foreach (var bestaand in rol.Rechten.ToList())
        {
            if (!rechten.Contains(bestaand.Naam, StringComparer.OrdinalIgnoreCase))
                rol.Rechten.Remove(bestaand);
        }
        foreach (var rechtNaam in rechten)
        {
            if (rol.Rechten.Any(recht => recht.Naam.Equals(rechtNaam, StringComparison.OrdinalIgnoreCase)))
                continue;
            var recht = await db.Rechten.SingleOrDefaultAsync(recht => recht.Naam == rechtNaam);
            if (recht is null)
            {
                recht = new Recht { Naam = rechtNaam };
                db.Rechten.Add(recht);
            }
            rol.Rechten.Add(recht);
        }
        await db.SaveChangesAsync();
    }

    private async Task BewaarAccount(Gebruiker gebruiker)
    {
        var bestaand = await users.FindByNameAsync(gebruiker.UserName!);
        if (bestaand is not null)
        {
            if (bestaand.RolID != gebruiker.RolID || bestaand.KlantID != gebruiker.KlantID)
                throw new InvalidOperationException($"Account '{gebruiker.UserName}' bestaat met een andere rol of klant.");

            if (!await users.CheckPasswordAsync(bestaand, TestWachtwoord))
            {
                var token = await users.GeneratePasswordResetTokenAsync(bestaand);
                var reset = await users.ResetPasswordAsync(bestaand, token, TestWachtwoord);
                if (!reset.Succeeded)
                    throw new InvalidOperationException(string.Join(" ", reset.Errors.Select(fout => fout.Description)));
            }
            return;
        }

        var result = await users.CreateAsync(gebruiker, TestWachtwoord);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(fout => fout.Description)));
    }
}
