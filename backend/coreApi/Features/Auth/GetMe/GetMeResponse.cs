namespace CoreApi.Features.Auth.GetMe;

public sealed record GetMeResponse(Guid Id, string Naam, string Rol, string[] Rechten,
    Guid? KlantId, bool HeeftKlanttoegang, string? Melding);
