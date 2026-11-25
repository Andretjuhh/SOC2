namespace CoreApi.Features.Gebruikers.CreateGebruiker;

public sealed record CreateGebruikerRequest(string Naam, string Gebruikersnaam, string Email,
    string Wachtwoord, bool IsActief = true, Guid? KlantId = null)
{
    public string? Valideer()
    {
        if (string.IsNullOrWhiteSpace(Naam))
            return "Vul een naam in.";
        if (string.IsNullOrWhiteSpace(Gebruikersnaam))
            return "Vul een gebruikersnaam in.";
        if (!System.Net.Mail.MailAddress.TryCreate(Email, out _))
            return "Vul een geldig e-mailadres in.";
        if (string.IsNullOrWhiteSpace(Wachtwoord) || Wachtwoord.Length > 1024)
            return "Vul een wachtwoord van maximaal 1024 tekens in.";
        return null;
    }
}
