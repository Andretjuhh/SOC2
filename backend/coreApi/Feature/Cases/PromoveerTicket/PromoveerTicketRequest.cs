using CoreApi.Domain;

namespace CoreApi.Features.Cases.PromoveerTicket;

// Met CaseID: ticket toevoegen aan een bestaande case.
// Zonder CaseID: er wordt een nieuwe case gemaakt met Titel, Beschrijving en Prioriteit.
public sealed record PromoveerTicketRequest(int? CaseID, string? Titel, string? Beschrijving,
    TicketPrioriteit? Prioriteit, byte[]? Versie)
{
    public string? Valideer()
    {
        if (Versie is null || Versie.Length == 0)
            return "Stuur de versie van het ticket mee.";
        if (CaseID is not null)
            return null;

        if (string.IsNullOrWhiteSpace(Titel))
            return "Vul een titel in voor de nieuwe case.";
        if (Titel.Trim().Length > 200)
            return "De titel mag maximaal 200 tekens zijn.";
        if (string.IsNullOrWhiteSpace(Beschrijving))
            return "Vul een beschrijving in voor de nieuwe case.";
        if (Prioriteit is null || !Enum.IsDefined(Prioriteit.Value))
            return "Kies een geldige prioriteit voor de nieuwe case.";
        return null;
    }
}
