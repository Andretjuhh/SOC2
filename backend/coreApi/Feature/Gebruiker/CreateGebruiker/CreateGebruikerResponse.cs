namespace CoreApi.Features.Gebruikers.CreateGebruiker;

public sealed record CreateGebruikerResponse(Guid Id, string Naam, string Gebruikersnaam,
    Guid RolId, Guid? KlantId, bool IsActief);
