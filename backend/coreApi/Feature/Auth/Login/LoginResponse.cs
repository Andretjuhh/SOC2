namespace CoreApi.Features.Auth.Login;

public sealed record LoginResponse(bool MfaVereist, string ChallengeId, DateTimeOffset VerlooptOp);
