using System.Security.Claims;
using CoreApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Alerts.GetAlert;

public static class GetAlertHandler
{
    public static async Task<IResult> Handle(
        int id,
        ClaimsPrincipal principal,
        SOC2DBContext db,
        CancellationToken cancellationToken)
    {
        var gebruiker = await AlertToegang.HaalGebruikerOp(principal, db, cancellationToken);
        if (gebruiker?.KlantID is not Guid klantId)
            return Results.Forbid();

        var alert = await db.Alerts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.AlertID == id && x.KlantID == klantId, cancellationToken);

        return alert is null
            ? Results.NotFound()
            : Results.Ok(AlertResponse.Van(alert));
    }
}