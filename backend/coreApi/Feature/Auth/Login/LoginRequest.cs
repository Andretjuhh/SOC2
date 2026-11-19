namespace CoreApi.Features.Auth.Login;

public sealed record LoginRequest(string Gebruikersnaam, string Wachtwoord)
{
    public string? Valideer()
    {
        if (string.IsNullOrWhiteSpace(Gebruikersnaam) || string.IsNullOrWhiteSpace(Wachtwoord))
            return "Vul een gebruikersnaam en wachtwoord in.";
        if (Wachtwoord.Length > 1024)
            return "Vul een wachtwoord van maximaal 1024 tekens in.";
        return null;
    }
}
