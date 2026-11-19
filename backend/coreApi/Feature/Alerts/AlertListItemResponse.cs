using CoreApi.Domain;

namespace CoreApi.Features.Alerts;

/// <summary>Represents an alert and its current assignment for the current user.</summary>
public sealed record AlertListItemResponse(
    int AlertId,
    string Title,
    string Severity,
    string Status,
    string? Owner,
    string AffectedAsset,
    DateTimeOffset DetectionTime,
    DateTimeOffset? ClosedTime,
    bool ClaimedByYou)
{
    public static AlertListItemResponse From(Alert alert, Guid currentUserId)
    {
        var owner = alert.AssignedUser?.Naam;
        if (string.IsNullOrWhiteSpace(owner))
            owner = alert.AssignedUser?.UserName;

        return new AlertListItemResponse(
            alert.AlertID,
            alert.Event,
            alert.Severity,
            alert.ClosedTime is null ? "New" : "Closed",
            owner,
            alert.AffectedAsset,
            AsUtcOffset(alert.DetectionTime),
            alert.ClosedTime is DateTime closedTime ? AsUtcOffset(closedTime) : null,
            alert.AssignedUserId == currentUserId);
    }

    private static DateTimeOffset AsUtcOffset(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
