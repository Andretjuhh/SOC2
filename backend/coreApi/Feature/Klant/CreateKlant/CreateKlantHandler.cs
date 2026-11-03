namespace CoreApi.Features.Klant.CreateKlant;

public static class CreateKlantHandler
{
    public static async Task<IResult> CreateKlantHandle(CreateKlantRequest request,
        CoreApi.Data.SOC2DBContext db, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Naam) || !System.Net.Mail.MailAddress.TryCreate(request.Email, out _))
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["klant"] = ["Vul een naam en geldig e-mailadres in."] });
        var klant = new CoreApi.Domain.Klant
        {
            naam = request.Naam.Trim(), email = request.Email.Trim(),
            telefoonnummer = request.Telefoonnummer, SLA = request.SLA, isActief = request.IsActief
        };
        db.Klanten.Add(klant);
        await db.SaveChangesAsync(ct);
        return Results.Created((string?)null, new CreateKlantResponse(
            klant.KlantID, klant.naam, klant.email, klant.telefoonnummer, klant.SLA, klant.isActief));
    }
}

