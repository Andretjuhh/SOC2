using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoreApi.Tests.AlertTests;

internal sealed record AlertTestScenario(
    int AlertId,
    Guid FirstUserId,
    Guid SecondUserId,
    ClaimsPrincipal FirstUser,
    ClaimsPrincipal SecondUser);

internal static class AlertTestDataFactory
{
    public static async Task<AlertTestScenario> CreateAsync(TicketTestDatabase database)
    {
        await using var db = database.NieuweContext();
        var suffix = Guid.NewGuid().ToString("N");
        var customer = new Klant
        {
            naam = "Alert test customer",
            email = $"{suffix}@example.test",
            isActief = true,
        };
        var role = new Rol { Naam = $"alert-test-{suffix}" };
        var first = CreateUser($"alert-first-{suffix}", "First analyst", customer, role);
        var second = CreateUser($"alert-second-{suffix}", "Second analyst", customer, role);
        var alert = new Alert
        {
            Event = "Alert test",
            Severity = "High",
            AffectedAsset = "test-host",
            DetectionTime = DateTime.UtcNow,
            Klant = customer,
        };

        db.Gebruikers.AddRange(first, second);
        db.Alerts.Add(alert);
        await db.SaveChangesAsync();

        return new AlertTestScenario(
            alert.AlertID,
            first.Id,
            second.Id,
            Principal(first.Id),
            Principal(second.Id));
    }

    private static Gebruiker CreateUser(string userName, string name, Klant customer, Rol role) => new()
    {
        UserName = userName,
        NormalizedUserName = userName.ToUpperInvariant(),
        Naam = name,
        IsActief = true,
        Klant = customer,
        Rol = role,
    };

    private static ClaimsPrincipal Principal(Guid userId) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "Test"));
}
