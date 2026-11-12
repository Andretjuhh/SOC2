namespace CoreApi.Domain;

public class Alert
{
    public int AlertID { get; set; }

    public string Event { get; set; } = string.Empty; //Soort van naam

    public string Severity { get; set; } = string.Empty; //Critical, High, Medium, Low

    public string AffectedAsset { get; set; } = string.Empty; //Eigenaar

    public DateTime DetectionTime { get; set; }

    public DateTime? ClosedTime { get; set; }

    public Guid KlantID { get; set; }
    public Klant Klant { get; set; } = null!;
}