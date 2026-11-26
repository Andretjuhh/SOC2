using CoreApi.Features.Alerts.CreateAlert;
using CoreApi.Features.Alerts.CrudAlert.ClaimAlert;
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

        group.MapPost("/{id:int}/claim", ClaimAlertHandler.Handle).WithName("ClaimAlert")
            .WithSummary("Claim an unassigned alert")
            .WithDescription("Atomically assigns the alert to the current user. A claim by another user returns 409 Conflict.")
            .Produces<AlertListItemResponse>().ProducesProblem(409).Produces(401).Produces(403).Produces(404);

        group.MapGet("", ListAlertsHandler.Handle).WithName("ListAlerts")
            .WithSummary("List alerts for the current customer's account")
            .Produces<IReadOnlyList<AlertListItemResponse>>().Produces(401).Produces(403);

        group.MapGet("/templates", ListAlertsHandler.HandleTemplates).WithName("ListAlertTemplates")
            .WithSummary("Get sample alert templates for the alert generator")
            .Produces<IReadOnlyList<AlertTestItem>>().Produces(401).Produces(403);

        group.MapGet("/{id:int}", GetAlertHandler.Handle).WithName("GetAlert")
            .Produces<AlertResponse>().Produces(401).Produces(403).Produces(404);
    }
}
