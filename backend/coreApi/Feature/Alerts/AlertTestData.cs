namespace CoreApi.Features.Alerts;

public static class AlertTestData
{
    public static IReadOnlyList<AlertResponse> Alerts { get; } =
    [
        new(1, "Unusual sign-in location", "Critical", "j.smith@acme.io",
            DateTime.UtcNow.AddMinutes(-8), null),
        new(2, "Multiple failed login attempts", "High", "10.24.8.119",
            DateTime.UtcNow.AddMinutes(-12), null),
        new(3, "Suspicious PowerShell activity", "High", "WS-0421",
            DateTime.UtcNow.AddMinutes(-20), null),
        new(4, "New inbox forwarding rule", "Medium", "m.chen@acme.io",
            DateTime.UtcNow.AddMinutes(-31), null),
        new(5, "Connection to a new domain", "Low", "10.24.3.54",
            DateTime.UtcNow.AddMinutes(-44), DateTime.UtcNow.AddMinutes(-35))
    ];
}
