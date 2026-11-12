namespace CoreApi.Features.Klant.CreateKlant;

public static class CreateKlantEndpoint
{
    public static RouteHandlerBuilder MapCreateKlantEndpoint(this RouteGroupBuilder routeGroupBuilder)
    {
        return routeGroupBuilder
            .MapPost("", CreateKlantHandler.CreateKlantHandle)
            .WithName("CreateKlant")
            .Produces<CreateKlantResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}