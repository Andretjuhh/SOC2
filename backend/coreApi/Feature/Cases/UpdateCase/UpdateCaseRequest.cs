using CoreApi.Domain;

namespace CoreApi.Features.Cases.UpdateCase;

// Alle velden zijn optioneel: alleen wat je meestuurt wordt aangepast
public sealed record UpdateCaseRequest(string? Titel, string? Beschrijving, TicketPrioriteit? Prioriteit,
    CaseStatus? Status, string? RootCause, byte[]? Versie)
{
    public string? Valideer()
    {
        if (Versie is null || Versie.Length == 0)
            return "Stuur de versie van de case mee.";
        if (Titel is not null && string.IsNullOrWhiteSpace(Titel))
            return "De titel mag niet leeg zijn.";
        if (Titel is not null && Titel.Trim().Length > 200)
            return "De titel mag maximaal 200 tekens zijn.";
        if (Beschrijving is not null && string.IsNullOrWhiteSpace(Beschrijving))
            return "De beschrijving mag niet leeg zijn.";
        if (Prioriteit is not null && !Enum.IsDefined(Prioriteit.Value))
            return "Kies een geldige prioriteit.";
        if (Status is not null && !Enum.IsDefined(Status.Value))
            return "Kies een geldige status.";
        return null;
    }
}
