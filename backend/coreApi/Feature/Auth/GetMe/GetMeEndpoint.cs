namespace CoreApi.Features.Auth.GetMe;

public static class GetMeEndpoint
{
    public static RouteHandlerBuilder MapGetMeEndpoint(this RouteGroupBuilder group) => group
        .MapGet("/me", GetMeHandler.Handle).WithName("GetMe").RequireAuthorization()
        .Produces<GetMeResponse>().Produces(401);
}
