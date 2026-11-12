namespace CoreApi.Features.Alerts.CreateAlert;

public sealed record CreateAlertRequest(
    string Event,
    string Severity,
    string AffectedAsset,
    DateTime DetectionTime,
    DateTime? ClosedTime = null)
{
    public string? Valideer()
    {
        if (string.IsNullOrWhiteSpace(Event))
            return "Vul een gebeurtenis in.";
        if (Event.Trim().Length > 200)
            return "De gebeurtenis mag maximaal 200 tekens zijn.";
        if (Severity is not ("Critical" or "High" or "Medium" or "Low"))
            return "Kies een geldige ernst.";
        if (string.IsNullOrWhiteSpace(AffectedAsset))
            return "Vul een getroffen asset in.";
        if (AffectedAsset.Trim().Length > 200)
            return "De getroffen asset mag maximaal 200 tekens zijn.";
        if (DetectionTime == default)
            return "Vul een geldige detectietijd in.";
        if (ClosedTime.HasValue && ClosedTime.Value < DetectionTime)
            return "De sluitingstijd mag niet vóór de detectietijd liggen.";

        return null;
    }
}