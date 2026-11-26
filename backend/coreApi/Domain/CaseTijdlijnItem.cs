namespace CoreApi.Domain;

// Fase van de aanval volgens MITRE ATT&CK (tactics). Optioneel per gebeurtenis.
public enum AanvalsFase
{
    Reconnaissance,
    InitialAccess,
    Execution,
    Persistence,
    PrivilegeEscalation,
    DefenseEvasion,
    CredentialAccess,
    Discovery,
    LateralMovement,
    Collection,
    CommandAndControl,
    Exfiltration,
    Impact
}

// Eén gebeurtenis in de aanvalstijdlijn van een case: wat deed de aanvaller, en wanneer gebeurde dat.
// Dit is iets anders dan de ticket-historie: die legt vast wat de analisten deden en wanneer.
// Alerts van gekoppelde tickets worden niet hier opgeslagen maar automatisch aan de tijdlijn toegevoegd.
public class CaseTijdlijnItem
{
    public int CaseTijdlijnItemID { get; set; }

    public int CaseID { get; set; }
    public Case Case { get; set; } = null!;

    // Wanneer de gebeurtenis plaatsvond (in UTC), niet wanneer hij is ingevoerd
    public DateTime Tijdstip { get; set; }

    public string Beschrijving { get; set; } = string.Empty;

    // Welk systeem of welke gebruiker was betrokken (optioneel)
    public string? Systeem { get; set; }

    public AanvalsFase? Fase { get; set; }

    // Wie heeft de gebeurtenis toegevoegd en wanneer
    public Guid ToegevoegdDoor { get; set; }
    public DateTime ToegevoegdOp { get; set; } = DateTime.UtcNow;
}
