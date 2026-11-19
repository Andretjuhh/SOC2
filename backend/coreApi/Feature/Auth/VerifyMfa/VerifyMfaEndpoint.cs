using CoreApi.Features.Auth.GetMe;

namespace CoreApi.Features.Auth.VerifyMfa;

public static class VerifyMfaEndpoint
{
    public static RouteHandlerBuilder MapVerifyMfaEndpoint(this RouteGroupBuilder group) => group
        .MapPost("/mfa/verify", VerifyMfaHandler.Handle).WithName("VerifyMfa").RequireRateLimiting("login")
        .Produces<GetMeResponse>().Produces(401).ProducesValidationProblem();
}
