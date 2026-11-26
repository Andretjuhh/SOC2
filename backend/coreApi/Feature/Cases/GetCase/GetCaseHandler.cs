using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Features.Tickets;

namespace CoreApi.Features.Cases.GetCase;

public static class GetCaseHandler
{
    public static async Task<IResult> Handle(int id, ClaimsPrincipal principal, SOC2DBContext db, CancellationToken ct)
    {
        var gebruiker = await TicketToegang.HaalGebruikerOp(principal, db, ct);
        if (gebruiker is null)
            return Results.Unauthorized();
        if (!TicketToegang.MagLezen(gebruiker))
            return Results.Problem(statusCode: 403, title: "Je hebt geen rechten om cases te bekijken.");

        // Een case van een andere klant geeft 404
        var zaak = await CaseResponse.LaadAsync(db, id, gebruiker.KlantID, ct);
        return zaak is null ? CaseResponse.NietGevonden() : Results.Ok(zaak);
    }
}
