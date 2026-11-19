using CoreApi.Data;
using CoreApi.Features.Alerts.CrudAlert.ClaimAlert;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreApi.Tests.AlertTests;

[Collection("Shared database")]
public sealed class ClaimAlertConcurrencyTests(TicketTestDatabase database)
{
    [Fact]
    public async Task ClaimingAtTheSameTime_AssignsAlertToOnlyOneUser()
    {
        var scenario = await AlertTestDataFactory.CreateAsync(database);
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var firstClaim = ClaimAfterSignal(scenario.AlertId, scenario.FirstUser, startSignal.Task);
        var secondClaim = ClaimAfterSignal(scenario.AlertId, scenario.SecondUser, startSignal.Task);
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
        var storedAlert = await db.Alerts.SingleAsync(alert => alert.AlertID == scenario.AlertId);
        Assert.True(storedAlert.AssignedUserId == scenario.FirstUserId ||
                    storedAlert.AssignedUserId == scenario.SecondUserId);
    }

    private async Task<IResult> ClaimAfterSignal(int alertId, System.Security.Claims.ClaimsPrincipal user, Task startSignal)
    {
        await startSignal;
        await using var db = database.NieuweContext();
        return await ClaimAlertHandler.Handle(alertId, user, db, CancellationToken.None);
    }

    private static int? StatusCode(IResult result) => ((IStatusCodeHttpResult)result).StatusCode;
}
