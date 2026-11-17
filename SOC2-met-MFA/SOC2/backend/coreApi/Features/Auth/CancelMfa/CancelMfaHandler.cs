using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace CoreApi.Features.Auth.CancelMfa;

public static class CancelMfaHandler
{
    public static async Task<IResult> Handle(CancelMfaRequest request, HttpContext context, MfaChallengeStore challenges)
    {
        if (!string.IsNullOrWhiteSpace(request.ChallengeId) && request.ChallengeId.Length <= 128)
            challenges.Consume(request.ChallengeId);
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        return Results.NoContent();
    }
}
