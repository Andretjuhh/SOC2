using CoreApi.Domain;

namespace CoreApi.Features.Cases.AanvalsTijdlijn;

// Een gebeurtenis toevoegen aan de aanvalstijdlijn. Tijdstip = wanneer het gebeurde, in UTC.
public sealed record TijdlijnItemRequest(DateTime? Tijdstip, string? Beschrijving, string? Systeem, AanvalsFase? Fase)
{
    public string? Valideer()
    {
        if (Tijdstip is null)
            return "Vul in wanneer de gebeurtenis plaatsvond.";
        // Een paar minuten speling voor klokverschil tussen browser en server
        if (Tijdstip.Value.ToUniversalTime() > DateTime.UtcNow.AddMinutes(5))
            return "Een gebeurtenis kan niet in de toekomst liggen.";
        if (string.IsNullOrWhiteSpace(Beschrijving))
            return "Beschrijf wat er gebeurde.";
        if (Beschrijving.Trim().Length > 1000)
            return "De beschrijving mag maximaal 1000 tekens zijn.";
        if (Systeem is not null && Systeem.Trim().Length > 200)
            return "Het systeem mag maximaal 200 tekens zijn.";
        if (Fase is not null && !Enum.IsDefined(Fase.Value))
            return "Kies een geldige fase.";
        return null;
    }
}
