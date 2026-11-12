using CoreApi.Domain;

namespace CoreApi.Features.Tickets.UpdateTicket;

// Alle velden zijn optioneel: alleen wat je meestuurt wordt aangepast
public sealed record UpdateTicketRequest(string? Titel, string? Beschrijving,
    TicketPrioriteit? Prioriteit, TicketCategorie? Categorie, TicketStatus? Status, byte[]? Versie)
{
    public string? Valideer()
    {
        if (Versie is null || Versie.Length == 0)
            return "Stuur de versie van het ticket mee.";
        if (Titel is not null && string.IsNullOrWhiteSpace(Titel))
            return "De titel mag niet leeg zijn.";
        if (Titel is not null && Titel.Trim().Length > 200)
            return "De titel mag maximaal 200 tekens zijn.";
        if (Beschrijving is not null && string.IsNullOrWhiteSpace(Beschrijving))
            return "De beschrijving mag niet leeg zijn.";
        if (Prioriteit is not null && !Enum.IsDefined(Prioriteit.Value))
            return "Kies een geldige prioriteit.";
        if (Categorie is not null && !Enum.IsDefined(Categorie.Value))
            return "Kies een geldige categorie.";
        if (Status == TicketStatus.Afgerond)
            return "Gebruik 'Ticket afronden' om een ticket af te sluiten.";
        if (Status is not null && !Enum.IsDefined(Status.Value))
            return "Kies een geldige status.";
        return null;
    }
}