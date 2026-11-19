using CoreApi.Data;
using CoreApi.Features.Alerts;
using CoreApi.Features.Alerts.CreateAlert;
using CoreApi.Features.Alerts.ListAlerts;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace CoreApi.Tests.AlertTests;

[Collection("Shared database")]
public sealed class AlertListRefreshTests(TicketTestDatabase database)
{
    [Fact]
    public async Task ListAlerts_AfterAlertCreated_IncludesTheNewAlert()
    {
        var scenario = await AlertTestDataFactory.CreateAsync(database);

        await using (var createDb = database.NieuweContext())
        {
            var createResult = await CreateAlertHandler.Handle(
                new CreateAlertRequest(
                    "Newly received test alert",
                    "High",
                    "test-host-2",
                    DateTime.UtcNow),
                scenario.FirstUser,
                createDb,
                CancellationToken.None);

            Assert.IsType<Created<AlertResponse>>(createResult);
        }

        await using var listDb = database.NieuweContext();
        var listResult = await ListAlertsHandler.Handle(
            scenario.SecondUser,
            listDb,
            CancellationToken.None);

        var response = Assert.IsType<Ok<List<AlertListItemResponse>>>(listResult).Value;
        Assert.Contains(response!, alert => alert.Title == "Newly received test alert");
    }
}
