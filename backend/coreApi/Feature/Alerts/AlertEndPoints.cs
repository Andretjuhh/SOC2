using CoreApi.Features.Alerts.CreateAlert;
using CoreApi.Features.Alerts.GetAlert;
using CoreApi.Features.Alerts.ListAlerts;

namespace CoreApi.Features.Alerts;

public static class AlertEndpoints
{
    public static void MapAlertEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/alerts").WithTags("Alerts").RequireAuthorization();

        group.MapPost("", CreateAlertHandler.Handle).WithName("CreateAlert")
            .Produces<AlertResponse>(201).Produces(401).Produces(403).ProducesValidationProblem();

        group.MapGet("", ListAlertsHandler.Handle).WithName("ListAlerts")
            .Produces<List<AlertResponse>>().Produces(401).Produces(403);

        group.MapGet("/{id:int}", GetAlertHandler.Handle).WithName("GetAlert")
            .Produces<AlertResponse>().Produces(401).Produces(403).Produces(404);
    }
}