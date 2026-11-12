using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CoreApi.Features.Alerts.CreateAlert;

public static class CreateAlertHandler
{
    public static async Task<IResult> Handle(
        [FromBody] CreateAlertRequest request,
        ClaimsPrincipal principal,
        SOC2DBContext db,
        CancellationToken cancellationToken)
    {
        var gebruiker = await AlertToegang.HaalGebruikerOp(principal, db, cancellationToken);
        if (gebruiker?.KlantID is not Guid klantId)
            return Results.Forbid();

        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["alert"] = [fout] });

        var alert = new Alert
        {
            Event = request.Event.Trim(),
            Severity = request.Severity,
            AffectedAsset = request.AffectedAsset.Trim(),
            DetectionTime = request.DetectionTime,
            KlantID = klantId
        };

        db.Alerts.Add(alert);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/alerts/{alert.AlertID}", AlertResponse.Van(alert));
    }
}