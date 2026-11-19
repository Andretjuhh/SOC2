using System.Security.Claims;
using CoreApi.Data;
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
        if (user?.KlantID is not Guid customerId)
            return Results.Forbid();

        var alerts = await db.Alerts.AsNoTracking()
            .Include(alert => alert.AssignedUser)
            .Where(alert => alert.KlantID == customerId)
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
        return user is null
            ? Results.Forbid()
            : Results.Ok(AlertTestData.Alerts);
    }
}