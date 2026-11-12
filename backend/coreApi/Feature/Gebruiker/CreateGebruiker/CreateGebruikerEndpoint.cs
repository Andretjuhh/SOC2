namespace CoreApi.Features.Gebruikers.CreateGebruiker;

public static class CreateGebruikerEndpoint
{
    public static RouteHandlerBuilder MapCreateGebruikerEndpoint(this RouteGroupBuilder group) => group
        .MapPost("", CreateGebruikerHandler.Handle).WithName("CreateGebruiker")
        .Produces<CreateGebruikerResponse>(201).Produces(409).ProducesValidationProblem();
}
