using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Alerts;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Alerts.ListAlerts;

public static class ListAlertsHandler
{
    public static async Task<IResult> Handle(
        ClaimsPrincipal principal,
        SOC2DBContext db,
        CancellationToken cancellationToken)
    {
        var user = await AlertToegang.HaalGebruikerOp(principal, db, cancellationToken);
        if (user is null || !AlertToegang.MagLezen(user))
            return Results.Forbid();

        var alerts = await db.Alerts.AsNoTracking()
            .Include(alert => alert.AssignedUser)
            .Where(alert => RechtenMatrix.IsBeheerder(user) || alert.KlantID == user.KlantID)
            .OrderByDescending(alert => alert.DetectionTime)
            .ToListAsync(cancellationToken);

        return Results.Ok(alerts.Select(alert => AlertListItemResponse.From(alert, user.Id)).ToList());
    }

    public static async Task<IResult> HandleTemplates(
        ClaimsPrincipal principal,
        SOC2DBContext db,
        CancellationToken cancellationToken)
    {
        var user = await AlertToegang.HaalGebruikerOp(principal, db, cancellationToken);
        return user is null || !AlertToegang.MagLezen(user)
            ? Results.Forbid()
            : Results.Ok(AlertTestData.Alerts);
    }
}
