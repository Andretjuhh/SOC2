using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
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
        if (gebruiker is null || !AlertToegang.MagLezen(gebruiker))
            return Results.Forbid();

        var alert = await db.Alerts.AsNoTracking()
            .SingleOrDefaultAsync(alert => alert.AlertID == id &&
                (RechtenMatrix.IsBeheerder(gebruiker) || alert.KlantID == gebruiker.KlantID), cancellationToken);

        return alert is null
            ? Results.NotFound()
            : Results.Ok(AlertResponse.Van(alert));
    }
}
