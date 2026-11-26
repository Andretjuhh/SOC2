namespace CoreApi.Domain;

public static class RechtenMatrix
{
    public const string GeenToegang = "Geen toegang";
    public const string Klant = "Klant";
    public const string Analist = "SOC-analist";
    public const string Beheerder = "Beheerder";

    public const string DashboardLezen = "dashboard.lezen";
    public const string AlertsLezen = "alerts.lezen";
    public const string AlertsSchrijven = "alerts.schrijven";
    public const string AlertsClaimen = "alerts.claimen";
    public const string GebruikersBeheren = "gebruikers.beheren";

    public static string[] VoorRol(string rol) => rol switch
    {
        Klant => [DashboardLezen, AlertsLezen, "tickets.lezen"],
        Analist => [DashboardLezen, AlertsLezen, AlertsSchrijven, AlertsClaimen, "tickets.lezen", "tickets.schrijven"],
        Beheerder => [DashboardLezen, AlertsLezen, AlertsSchrijven, AlertsClaimen, "tickets.lezen", "tickets.schrijven", GebruikersBeheren],
        _ => []
    };

    public static bool HeeftRecht(Gebruiker gebruiker, string recht) =>
        gebruiker.IsActief && gebruiker.Rol.Naam != GeenToegang
        && gebruiker.Rol.Rechten.Any(bestaand => bestaand.Naam.Equals(recht, StringComparison.OrdinalIgnoreCase));

    public static bool IsBeheerder(Gebruiker gebruiker) =>
        gebruiker.Rol.Naam == Beheerder && HeeftRecht(gebruiker, GebruikersBeheren);

    public static bool HeeftKlanttoegang(Gebruiker gebruiker) =>
        IsBeheerder(gebruiker) || gebruiker.Klant is { isActief: true };
}
