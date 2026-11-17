using Microsoft.AspNetCore.Identity;
using CoreApi.Domain;

namespace CoreApi.Features.Auth.Login;

public static class LoginHandler
{
    public static async Task<IResult> Handle(LoginRequest request, UserManager<Gebruiker> users,
        SignInManager<Gebruiker> signIn, MfaChallengeStore challenges)
    {
        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["login"] = [fout] });

        var gebruiker = await ControleerInloggegevens(request, users, signIn, challenges);
        if (gebruiker is null)
            return Results.Problem(statusCode: 401, title: "Ongeldige inloggegevens.");

        var challenge = challenges.Create(gebruiker.Id);
        return Results.Accepted(value: new LoginResponse(true, challenge.Id, challenge.VerlooptOp));
    }

    private static async Task<Gebruiker?> ControleerInloggegevens(LoginRequest request,
        UserManager<Gebruiker> users, SignInManager<Gebruiker> signIn, MfaChallengeStore challenges)
    {
        var gebruiker = await users.FindByNameAsync(request.Gebruikersnaam.Trim());
        if (gebruiker is null || !gebruiker.IsActief)
        {
            users.PasswordHasher.VerifyHashedPassword(new Gebruiker(), challenges.DummyHash, request.Wachtwoord);
            return null;
        }
        var result = await signIn.CheckPasswordSignInAsync(gebruiker, request.Wachtwoord, lockoutOnFailure: true);
        return result.Succeeded ? gebruiker : null;
    }
}
