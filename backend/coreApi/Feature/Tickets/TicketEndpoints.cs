using CoreApi.Features.Tickets.CreateTicket;
using CoreApi.Features.Tickets.GetTicket;
using CoreApi.Features.Tickets.GetTicketHistorie;
using CoreApi.Features.Tickets.ResolveTicket;
using CoreApi.Features.Tickets.UpdateTicket;

namespace CoreApi.Features.Tickets;

public static class TicketEndpoints
{
    public static void MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        // Alle ticket-endpoints vereisen een ingelogde gebruiker
        var group = app.MapGroup("/api/tickets").WithTags("Tickets").RequireAuthorization();

        group.MapPost("", CreateTicketHandler.Handle).WithName("CreateTicket")
            .Produces<TicketResponse>(201).Produces(401).Produces(403).ProducesValidationProblem();

        group.MapGet("/{id:int}", GetTicketHandler.Handle).WithName("GetTicket")
            .Produces<TicketResponse>().Produces(401).Produces(403).Produces(404);

        group.MapPatch("/{id:int}", UpdateTicketHandler.Handle).WithName("UpdateTicket")
            .Produces<TicketResponse>().Produces(401).Produces(403).Produces(404).Produces(409)
            .ProducesValidationProblem();

        group.MapPost("/{id:int}/afronden", ResolveTicketHandler.Handle).WithName("ResolveTicket")
            .Produces<TicketResponse>().Produces(401).Produces(403).Produces(404).Produces(409)
            .ProducesValidationProblem();

        group.MapGet("/{id:int}/historie", GetTicketHistorieHandler.Handle).WithName("GetTicketHistorie")
            .Produces<List<TicketHistorieResponse>>().Produces(401).Produces(403).Produces(404);
    }
}