using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CoreApi.Data;
using CoreApi.Features.Auth;
using CoreApi.Features.Gebruikers.SetupMfa;
using CoreApi.Features.Gebruikers.CreateGebruiker;
using CoreApi.Features.Klant.CreateKlant;
using CoreApi.Features.Alerts;
using CoreApi.Features.Cases;
using CoreApi.Features.Tickets;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDbContext<SOC2DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSOC2Identity(builder.Environment.IsDevelopment());
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MfaChallengeStore>();
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

var app = builder.Build();

// Oude tickets bijwerken: alsnog aan hun alert koppelen (zie AlertKoppeling) en de statusregel
// toepassen (zie TicketRegels). Lukt het niet, dan start de API gewoon.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SOC2DBContext>();
        var gekoppeld = await AlertKoppeling.KoppelOudeTicketsAsync(db);
        if (gekoppeld > 0)
            app.Logger.LogInformation("{Aantal} oude ticket(s) gekoppeld aan hun alert.", gekoppeld);
        await TicketRegels.CorrigeerBestaandeTicketsAsync(db);
    }
    catch (Exception fout)
    {
        app.Logger.LogWarning(fout, "Ticketstatussen konden niet worden gecorrigeerd.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGroup("/api/gebruikers").WithTags("Gebruikers").MapCreateGebruikerEndpoint();
    app.MapGroup("/api/gebruikers").WithTags("Gebruikers").MapSetupMfaEndpoint()
        .RequireRateLimiting("login");
    app.MapGroup("/api/klanten").WithTags("Klanten").MapCreateKlantEndpoint();
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapTicketEndpoints();
app.MapAlertEndpoints();
app.MapCaseEndpoints();

app.MapGet("/health", () => TypedResults.Ok(new { status = "Healthy" }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.Run();
