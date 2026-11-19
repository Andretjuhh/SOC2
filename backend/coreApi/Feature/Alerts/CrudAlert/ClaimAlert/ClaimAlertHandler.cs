using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Features.Alerts;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Features.Alerts.CrudAlert.ClaimAlert;

public static class ClaimAlertHandler
{
    public static async Task<IResult> Handle(int id, ClaimsPrincipal principal, SOC2DBContext db, CancellationToken cancellationToken)
    {
        var user = await AlertToegang.HaalGebruikerOp(principal, db, cancellationToken);
        if (user?.KlantID is not Guid customerId)
            return Results.Forbid();

        var affectedRows = await db.Alerts
            .Where(alert =>
                alert.AlertID == id &&
                alert.KlantID == customerId &&
                alert.AssignedUserId == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(alert => alert.AssignedUserId, user.Id),
                cancellationToken);

        var alert = await db.Alerts.AsNoTracking()
            .Include(item => item.AssignedUser)
            .SingleOrDefaultAsync(
                item => item.AlertID == id && item.KlantID == customerId,
                cancellationToken);

        if (alert is null)
            return Results.NotFound();

        if (affectedRows == 1 || alert.AssignedUserId == user.Id)
            return Results.Ok(AlertListItemResponse.From(alert, user.Id));

        var owner = alert.AssignedUser?.Naam;
        if (string.IsNullOrWhiteSpace(owner))
            owner = alert.AssignedUser?.UserName ?? "another user";

        return Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Alert already claimed",
            detail: $"This alert was just claimed by {owner}. Refresh the list to see the latest status.");
    }
}
