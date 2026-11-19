namespace coreApi.Alerts;

public static class AlertEndpoints
{
    private static readonly Alert[] DemoAlerts =
    [
        new(1, "acme", "Unusual sign-in location", "Identity Protection", AlertPriority.Critical, AlertStatus.Investigating, new DateTimeOffset(2026, 11, 3, 10, 42, 18, TimeSpan.Zero)),
        new(2, "acme", "Multiple failed login attempts", "Microsoft Entra ID", AlertPriority.High, AlertStatus.Open, new DateTimeOffset(2026, 11, 3, 10, 38, 51, TimeSpan.Zero)),
        new(3, "acme", "Suspicious PowerShell activity", "Endpoint Detection", AlertPriority.High, AlertStatus.Investigating, new DateTimeOffset(2026, 11, 3, 10, 31, 6, TimeSpan.Zero)),
        new(4, "contoso", "New inbox forwarding rule", "Email Security", AlertPriority.Medium, AlertStatus.Open, new DateTimeOffset(2026, 11, 3, 10, 24, 33, TimeSpan.Zero)),
        new(5, "contoso", "Connection to a new domain", "Network Monitor", AlertPriority.Low, AlertStatus.Resolved, new DateTimeOffset(2026, 11, 2, 10, 12, 9, TimeSpan.Zero))
    ];

    public static void MapAlertEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/filtered-alerts", GetAlerts)
            .WithName("GetAlerts")
            .WithTags("Alerts")
            .WithSummary("Get alerts filtered by priority, customer, status, and UTC date.")
            .WithDescription("The from and to dates are inclusive. Results are ordered newest first and paginated.");
    }

    private static IResult GetAlerts(
        string? priority,
        string? customerId,
        string? status,
        DateOnly? from,
        DateOnly? to,
        int page = 1,
        int pageSize = 20)
    {
        var errors = new Dictionary<string, string[]>();
        AlertPriority? parsedPriority = null;
        AlertStatus? parsedStatus = null;

        if (priority is not null)
        {
            if (Enum.TryParse<AlertPriority>(priority, ignoreCase: true, out var value) && Enum.IsDefined(value))
            {
                parsedPriority = value;
            }
            else
            {
                errors["priority"] = ["Priority must be Critical, High, Medium, or Low."];
            }
        }

        if (status is not null)
        {
            if (Enum.TryParse<AlertStatus>(status, ignoreCase: true, out var value) && Enum.IsDefined(value))
            {
                parsedStatus = value;
            }
            else
            {
                errors["status"] = ["Status must be Open, Investigating, or Resolved."];
            }
        }

        if (customerId is not null && string.IsNullOrWhiteSpace(customerId))
        {
            errors["customerId"] = ["Customer ID cannot be empty."];
        }

        if (from is not null && to is not null && from > to)
        {
            errors["to"] = ["The to date must be on or after the from date."];
        }

        if (page < 1)
        {
            errors["page"] = ["Page must be greater than zero."];
        }

        if (pageSize is < 1 or > 100)
        {
            errors["pageSize"] = ["Page size must be between 1 and 100."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var filteredAlerts = DemoAlerts
            .Where(alert => parsedPriority is null || alert.Priority == parsedPriority)
            .Where(alert => customerId is null || string.Equals(alert.CustomerId, customerId.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(alert => parsedStatus is null || alert.Status == parsedStatus)
            .Where(alert => from is null || DateOnly.FromDateTime(alert.CreatedAt.UtcDateTime) >= from)
            .Where(alert => to is null || DateOnly.FromDateTime(alert.CreatedAt.UtcDateTime) <= to)
            .OrderByDescending(alert => alert.CreatedAt)
            .ToArray();

        var items = filteredAlerts
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return TypedResults.Ok(new
        {
            page,
            pageSize,
            totalCount = filteredAlerts.Length,
            items
        });
    }
}
