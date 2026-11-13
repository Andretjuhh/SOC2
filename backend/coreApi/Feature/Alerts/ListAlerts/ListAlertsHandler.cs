using CoreApi.Features.Alerts;

namespace CoreApi.Features.Alerts.ListAlerts;

public static class ListAlertsHandler
{
    public static IResult Handle() => Results.Ok(AlertTestData.Alerts);
}