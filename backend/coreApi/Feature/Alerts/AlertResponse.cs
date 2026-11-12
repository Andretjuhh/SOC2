using CoreApi.Domain;

namespace CoreApi.Features.Alerts;

public sealed record AlertResponse(
    int AlertID,
    string Event,
    string Severity,
    string AffectedAsset,
    DateTime DetectionTime,
    DateTime? ClosedTime)
{
    public static AlertResponse Van(Alert alert) => new(
        alert.AlertID,
        alert.Event,
        alert.Severity,
        alert.AffectedAsset,
        alert.DetectionTime,
        alert.ClosedTime);
}
