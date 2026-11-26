export const CASE_STATUSSEN = [
  { value: 'Open', label: 'Open' },
  { value: 'InOnderzoek', label: 'In onderzoek' },
  { value: 'Afgesloten', label: 'Afgesloten' },
]

export function caseStatusLabel(status) {
  return CASE_STATUSSEN.find((optie) => optie.value === status)?.label ?? status
}

// MITRE ATT&CK-tactieken; waarden gelijk aan de enum AanvalsFase in backend/coreApi/Domain/CaseTijdlijnItem.cs
export const AANVALSFASES = [
  { value: 'Reconnaissance', label: 'Verkenning (Reconnaissance)' },
  { value: 'InitialAccess', label: 'Eerste toegang (Initial Access)' },
  { value: 'Execution', label: 'Uitvoering (Execution)' },
  { value: 'Persistence', label: 'Verankering (Persistence)' },
  { value: 'PrivilegeEscalation', label: 'Rechtenverhoging (Privilege Escalation)' },
  { value: 'DefenseEvasion', label: 'Ontwijken beveiliging (Defense Evasion)' },
  { value: 'CredentialAccess', label: 'Inloggegevens buitmaken (Credential Access)' },
  { value: 'Discovery', label: 'Omgeving verkennen (Discovery)' },
  { value: 'LateralMovement', label: 'Zijwaartse beweging (Lateral Movement)' },
  { value: 'Collection', label: 'Gegevens verzamelen (Collection)' },
  { value: 'CommandAndControl', label: 'Command and Control' },
  { value: 'Exfiltration', label: 'Gegevens weggesluisd (Exfiltration)' },
  { value: 'Impact', label: 'Schade (Impact)' },
]

export function faseLabel(fase) {
  return AANVALSFASES.find((optie) => optie.value === fase)?.label ?? fase
}
