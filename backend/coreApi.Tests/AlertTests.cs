using System.Security.Claims;
using CoreApi.Data;
using CoreApi.Domain;
using CoreApi.Features.Alerts.CrudAlert.ClaimAlert;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreApi.Tests;

[Collection("Shared database")]
public sealed class AlertTests(TicketTestDatabase database)
{
    [Fact]
    public async Task ClaimingAtTheSameTime_AssignsAlertToOnlyOneUser()
    {
        var (alertId, firstUser, secondUser, firstUserId, secondUserId) = await CreateAlertAndUsers();
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var firstClaim = ClaimAfterSignal(alertId, firstUser, startSignal.Task);
        var secondClaim = ClaimAfterSignal(alertId, secondUser, startSignal.Task);
        startSignal.SetResult();

        var results = await Task.WhenAll(firstClaim, secondClaim);

        Assert.Equal(1, results.Count(result => StatusCode(result) == StatusCodes.Status200OK));
        var conflict = Assert.Single(
            results,
            result => StatusCode(result) == StatusCodes.Status409Conflict);
        var problem = Assert.IsType<ProblemHttpResult>(conflict).ProblemDetails;
        Assert.Equal("Alert already claimed", problem.Title);
        Assert.Contains("claimed by", problem.Detail);

        await using var db = database.NieuweContext();
        var storedAlert = await db.Alerts.SingleAsync(alert => alert.AlertID == alertId);
        Assert.True(storedAlert.AssignedUserId == firstUserId || storedAlert.AssignedUserId == secondUserId);
    }

    private async Task<(int AlertId, ClaimsPrincipal FirstUser, ClaimsPrincipal SecondUser, Guid FirstUserId, Guid SecondUserId)>
        CreateAlertAndUsers()
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
        role.Rechten.Add(await db.Rechten.SingleOrDefaultAsync(recht => recht.Naam == RechtenMatrix.AlertsClaimen)
            ?? new Recht { Naam = RechtenMatrix.AlertsClaimen });
        var first = CreateUser($"alert-first-{suffix}", "First analyst", customer, role);
        var second = CreateUser($"alert-second-{suffix}", "Second analyst", customer, role);
        var alert = new Alert
        {
            Event = "Concurrent claim test",
            Severity = "High",
            AffectedAsset = "test-host",
            DetectionTime = DateTime.UtcNow,
            Klant = customer,
        };

        db.Gebruikers.AddRange(first, second);
        db.Alerts.Add(alert);
        await db.SaveChangesAsync();

        return (
            alert.AlertID,
            Principal(first.Id),
            Principal(second.Id),
            first.Id,
            second.Id);
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

    private async Task<IResult> ClaimAfterSignal(int alertId, ClaimsPrincipal user, Task startSignal)
    {
        await startSignal;
        await using var db = database.NieuweContext();
        return await ClaimAlertHandler.Handle(alertId, user, db, CancellationToken.None);
    }

    private static ClaimsPrincipal Principal(Guid userId) =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "Test"));

    private static int? StatusCode(IResult result) => ((IStatusCodeHttpResult)result).StatusCode;
}
