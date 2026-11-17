namespace CoreApi.Features.Klant.CreateKlant;

public sealed record CreateKlantRequest(string Naam, string Email, string Telefoonnummer, string SLA, bool IsActief);
