namespace CoreApi.Features.Klant.CreateKlant;

public sealed record class CreateKlantResponse(
    Guid UUID,
    string Name,
    string email,
    string telefoonnummer,
    string SLA,
    Boolean isActief
    
  );
