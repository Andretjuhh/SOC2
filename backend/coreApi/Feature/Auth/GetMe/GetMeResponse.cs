using CoreApi.Domain;

namespace CoreApi.Features.Auth.GetMe;

public sealed record GetMeResponse(Guid Id, string Naam, string Rol, string[] Rechten,
    Guid? KlantId, bool HeeftKlanttoegang, string? Melding,
    bool MagDashboard = false, bool MagGebruikersBeheren = false)
{
    public static GetMeResponse Van(Gebruiker gebruiker)
    {
        var klanttoegang = RechtenMatrix.HeeftKlanttoegang(gebruiker);
        var melding = gebruiker.Rol.Naam == RechtenMatrix.GeenToegang || gebruiker.Rol.Rechten.Count == 0
            ? "Je account wacht op toegang. Een beheerder moet eerst een rol toekennen."
            : klanttoegang ? null : "Er is geen actieve klanttoegang gekoppeld.";
        return new GetMeResponse(gebruiker.Id, gebruiker.Naam, gebruiker.Rol.Naam,
            gebruiker.Rol.Rechten.Select(recht => recht.Naam).Order().ToArray(), gebruiker.KlantID,
            klanttoegang, melding,
            klanttoegang && RechtenMatrix.HeeftRecht(gebruiker, RechtenMatrix.DashboardLezen),
            RechtenMatrix.IsBeheerder(gebruiker));
    }
}
