using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        if (gebruiker is null || !AlertToegang.MagSchrijven(gebruiker))
            return Results.Forbid();

        var klantId = request.KlantId ?? gebruiker.KlantID;
        if (!RechtenMatrix.IsBeheerder(gebruiker) && klantId != gebruiker.KlantID)
            return Results.Forbid();
        if (klantId is null || !await db.Klanten.AnyAsync(klant => klant.KlantID == klantId && klant.isActief, cancellationToken))
            return Results.BadRequest(new { melding = "Kies een actieve klant." });

        var fout = request.Valideer();
        if (fout is not null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["alert"] = [fout] });

        var alert = new Alert
        {
            Event = request.Event.Trim(),
            Severity = request.Severity,
            AffectedAsset = request.AffectedAsset.Trim(),
            DetectionTime = request.DetectionTime,
            ClosedTime = request.ClosedTime,
            KlantID = klantId.Value
        };

        db.Alerts.Add(alert);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/alerts/{alert.AlertID}", AlertResponse.Van(alert));
    }
}
