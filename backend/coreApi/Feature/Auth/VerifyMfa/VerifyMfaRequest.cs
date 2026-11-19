using System.Text.RegularExpressions;

namespace CoreApi.Features.Auth.VerifyMfa;

public sealed partial record VerifyMfaRequest(string ChallengeId, string Code)
{
    public string? Valideer()
    {
        if (string.IsNullOrWhiteSpace(ChallengeId) || ChallengeId.Length > 128)
            return "Ongeldige verificatiesessie.";
        if (string.IsNullOrWhiteSpace(Code) || !ZesCijfers().IsMatch(Code.Trim()))
            return "Vul een code van 6 cijfers in.";
        return null;
    }

    [GeneratedRegex("^[0-9]{6}$")]
    private static partial Regex ZesCijfers();
}
