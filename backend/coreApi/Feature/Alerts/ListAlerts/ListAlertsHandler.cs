namespace CoreApi.Features.Alerts.ListAlerts;

public static class ListAlertsHandler
{
    public static IResult Handle()
    {
        return Results.Ok(AlertTestData.Alerts);
    }
}