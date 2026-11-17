namespace CoreApi.Features.Auth.CancelMfa;

public sealed record CancelMfaRequest(string? ChallengeId);

public static class CancelMfaEndpoint
{
    public static RouteHandlerBuilder MapCancelMfaEndpoint(this RouteGroupBuilder group) => group
        .MapPost("/mfa/cancel", CancelMfaHandler.Handle).WithName("CancelMfa").RequireRateLimiting("login")
        .Produces(204);
}
