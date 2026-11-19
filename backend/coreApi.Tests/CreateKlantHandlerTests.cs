using CoreApi.Data;
using CoreApi.Features.Klant.CreateKlant;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreApi.Tests;

public sealed class CreateKlantHandlerTests
{
    [Theory]
    [InlineData(null, "Standaard")]
    [InlineData("0612345678", null)]
    public async Task OntbrekendTelefoonnummerOfSla_GeeftValidatiefout(string? telefoonnummer, string? sla)
    {
        var options = new DbContextOptionsBuilder<SOC2DBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new SOC2DBContext(options);
        var request = new CreateKlantRequest(
            "Testklant", "test@example.test", telefoonnummer, sla, true);

        var result = await CreateKlantHandler.CreateKlantHandle(request, db, CancellationToken.None);

        Assert.Equal(400, ((IStatusCodeHttpResult)result).StatusCode);
    }
}
