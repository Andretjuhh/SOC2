namespace CoreApi.Features.Auth.Logout;

public static class LogoutEndpoint
{
    public static RouteHandlerBuilder MapLogoutEndpoint(this RouteGroupBuilder group) => group
        .MapPost("/logout", (Delegate)LogoutHandler.Handle).WithName("Logout").RequireAuthorization().Produces(204);
}
