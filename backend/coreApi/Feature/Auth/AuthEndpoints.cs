using CoreApi.Features.Auth.GetMe;
using CoreApi.Features.Auth.Login;
using CoreApi.Features.Auth.Logout;

namespace CoreApi.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authenticatie");
        group.MapLoginEndpoint();
        group.MapGetMeEndpoint();
        group.MapLogoutEndpoint();
    }
}
