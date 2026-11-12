using System.Security.Claims;
using CoreApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Alerts.ListAlerts;

public static class ListAlertsHandler
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        SOC2DBContext db,
        CancellationToken cancellationToken)
    {
        var gebruiker = await AlertToegang.HaalGebruikerOp(principal, db, cancellationToken);
        if (gebruiker?.KlantID is not Guid klantId)
            return Results.Forbid();

        var alerts = await db.Alerts.AsNoTracking()
            .Where(x => x.KlantID == klantId)
            .OrderByDescending(x => x.DetectionTime)
            .Select(x => new AlertResponse(
                x.AlertID,
                x.Event,
                x.Severity,
                x.AffectedAsset,
                x.DetectionTime))
            .ToListAsync(cancellationToken);

        return Results.Ok(alerts);
    }
}