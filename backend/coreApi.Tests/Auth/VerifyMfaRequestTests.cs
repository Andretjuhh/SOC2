using CoreApi.Features.Auth.VerifyMfa;
using Xunit;

namespace CoreApi.Tests;

public sealed class VerifyMfaRequestTests
{
    [Theory]
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData(" 123456 ")]
    public void GeldigeCode_GeeftGeenFout(string code) =>
        Assert.Null(new VerifyMfaRequest("abc123", code).Valideer());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12345a")]
    [InlineData("+12345")]
    [InlineData("12.456")]
    [InlineData("١٢٣٤٥٦")] // niet-ASCII cijfers
    public void OngeldigeCode_GeeftFoutmelding(string? code) =>
        Assert.Equal("Vul een code van 6 cijfers in.", new VerifyMfaRequest("abc123", code!).Valideer());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void LeegChallengeId_GeeftFoutmelding(string? id) =>
        Assert.Equal("Ongeldige verificatiesessie.", new VerifyMfaRequest(id!, "123456").Valideer());

    [Fact]
    public void TeLangChallengeId_GeeftFoutmelding() =>
        Assert.NotNull(new VerifyMfaRequest(new string('A', 129), "123456").Valideer());
}
