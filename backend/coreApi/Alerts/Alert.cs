namespace coreApi.Alerts;

public sealed record Alert(
    int Id,
    string CustomerId,
    string Title,
    string Source,
    AlertPriority Priority,
    AlertStatus Status,
    DateTimeOffset CreatedAt);

public enum AlertPriority
{
    Critical,
    High,
    Medium,
    Low
}

public enum AlertStatus
{
    Open,
    Investigating,
    Resolved
}
