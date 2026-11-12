using CoreApi.Domain;

namespace CoreApi.Features.Tickets.CreateTicket;

public sealed record CreateTicketRequest(string Titel, string Beschrijving,
    TicketPrioriteit Prioriteit, TicketCategorie Categorie)
{
    public string? Valideer()
    {
        if (string.IsNullOrWhiteSpace(Titel))
            return "Vul een titel in.";
        if (Titel.Trim().Length > 200)
            return "De titel mag maximaal 200 tekens zijn.";
        if (string.IsNullOrWhiteSpace(Beschrijving))
            return "Vul een beschrijving in.";
        if (!Enum.IsDefined(Prioriteit))
            return "Kies een geldige prioriteit.";
        if (!Enum.IsDefined(Categorie))
            return "Kies een geldige categorie.";
        return null;
    }
}