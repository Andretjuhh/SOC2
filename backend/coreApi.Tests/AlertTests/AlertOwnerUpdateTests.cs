using CoreApi.Data;
using CoreApi.Features.Alerts;
using CoreApi.Features.Alerts.CrudAlert.ClaimAlert;
using CoreApi.Features.Alerts.ListAlerts;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace CoreApi.Tests.AlertTests;

[Collection("Shared database")]
public sealed class AlertOwnerUpdateTests(TicketTestDatabase database)
{
    [Fact]
    public async Task ClaimAlert_ReturnsTheNewOwnerImmediately()
    {
        var scenario = await AlertTestDataFactory.CreateAsync(database);

        await using var db = database.NieuweContext();
        var result = await ClaimAlertHandler.Handle(
            scenario.AlertId,
            scenario.FirstUser,
            db,
            CancellationToken.None);

        var response = Assert.IsType<Ok<AlertListItemResponse>>(result).Value;
        Assert.NotNull(response);
        Assert.Equal("First analyst", response.Owner);
        Assert.True(response.ClaimedByYou);
    }

    [Fact]
    public async Task ListAlerts_AfterClaim_ReturnsTheUpdatedOwnerOnNextRefresh()
    {
        var scenario = await AlertTestDataFactory.CreateAsync(database);

        await using (var claimDb = database.NieuweContext())
        {
            var claimResult = await ClaimAlertHandler.Handle(
                scenario.AlertId,
                scenario.FirstUser,
                claimDb,
                CancellationToken.None);
            Assert.IsType<Ok<AlertListItemResponse>>(claimResult);
        }

        await using var listDb = database.NieuweContext();
        var listResult = await ListAlertsHandler.Handle(
            scenario.SecondUser,
            listDb,
            CancellationToken.None);

        var response = Assert.IsType<Ok<List<AlertListItemResponse>>>(listResult).Value;
        var alert = Assert.Single(response!);
        Assert.Equal("First analyst", alert.Owner);
        Assert.False(alert.ClaimedByYou);
    }
}
