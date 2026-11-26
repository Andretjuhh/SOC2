using CoreApi.Features.Cases.GetCase;
using CoreApi.Features.Cases.ListCases;
using CoreApi.Features.Cases.PromoveerTicket;
using CoreApi.Features.Cases.AanvalsTijdlijn;
using CoreApi.Features.Cases.UpdateCase;

namespace CoreApi.Features.Cases;

public static class CaseEndpoints
{
    public static void MapCaseEndpoints(this IEndpointRouteBuilder app)
    {
        // Alle case-endpoints vereisen een ingelogde gebruiker
        var group = app.MapGroup("/api/cases").WithTags("Cases").RequireAuthorization();

        group.MapGet("", ListCasesHandler.Handle).WithName("ListCases")
            .Produces<List<CaseOverzichtResponse>>().Produces(401).Produces(403);

        group.MapGet("/{id:int}", GetCaseHandler.Handle).WithName("GetCase")
            .Produces<CaseResponse>().Produces(401).Produces(403).Produces(404);

        group.MapPatch("/{id:int}", UpdateCaseHandler.Handle).WithName("UpdateCase")
            .Produces<CaseResponse>().Produces(401).Produces(403).Produces(404).Produces(409)
            .ProducesValidationProblem();

        // Aanvalstijdlijn: gebeurtenis toevoegen of een verkeerd ingevoerde gebeurtenis verwijderen
        group.MapPost("/{id:int}/tijdlijn", TijdlijnHandlers.VoegToe).WithName("VoegTijdlijnItemToe")
            .Produces<CaseResponse>().Produces(401).Produces(403).Produces(404).Produces(409)
            .ProducesValidationProblem();

        group.MapDelete("/{id:int}/tijdlijn/{itemId:int}", TijdlijnHandlers.Verwijder).WithName("VerwijderTijdlijnItem")
            .Produces<CaseResponse>().Produces(401).Produces(403).Produces(404).Produces(409);

        // Een ticket promoveren: naar een nieuwe case of toevoegen aan een bestaande case
        app.MapPost("/api/tickets/{id:int}/promoveren", PromoveerTicketHandler.Handle)
            .WithTags("Cases").RequireAuthorization().WithName("PromoveerTicket")
            .Produces<CaseResponse>().Produces(401).Produces(403).Produces(404).Produces(409)
            .ProducesValidationProblem();
    }
}
