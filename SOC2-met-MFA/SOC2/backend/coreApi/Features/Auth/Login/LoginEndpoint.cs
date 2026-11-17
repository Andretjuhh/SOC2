namespace CoreApi.Features.Auth.Login;

public static class LoginEndpoint
{
    public static RouteHandlerBuilder MapLoginEndpoint(this RouteGroupBuilder group) => group
        .MapPost("/login", LoginHandler.Handle).WithName("Login").RequireRateLimiting("login")
        .Produces<LoginResponse>(202).Produces(401).ProducesValidationProblem();
}
