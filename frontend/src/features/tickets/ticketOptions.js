// Waarden moeten exact overeenkomen met de enums in backend/coreApi/Domain/Ticket.cs
export const PRIORITEITEN = [
  { value: 'Laag', label: 'Laag' },
  { value: 'Midden', label: 'Midden' },
  { value: 'Hoog', label: 'Hoog' },
  { value: 'Kritiek', label: 'Kritiek' },
]

export const CATEGORIEEN = [
  { value: 'Malware', label: 'Malware' },
  { value: 'Phishing', label: 'Phishing' },
  { value: 'OngeautoriseerdeToegang', label: 'Ongeautoriseerde toegang' },
  { value: 'DDoS', label: 'DDoS' },
  { value: 'Datalek', label: 'Datalek' },
  { value: 'Overig', label: 'Overig' },
]

export const TITEL_MAX_LENGTE = 200

const PRIORITEIT_PER_SEVERITY = {
  Critical: 'Kritiek',
  High: 'Hoog',
  Medium: 'Midden',
  Low: 'Laag',
}

// Eerste trefwoord dat in de alertnaam voorkomt bepaalt de voorgestelde categorie.
const CATEGORIE_TREFWOORDEN = [
  ['Phishing', ['phishing', 'oauth consent', 'inbox forwarding']],
  ['Malware', ['malware', 'powershell', 'base64', 'c2', 'ransomware', 'endpoint protection']],
  ['DDoS', ['ddos', 'traffic spike', 'flood']],
  ['Datalek', ['exfiltration', 'data leak', 'sensitive file', 'dns tunneling', 'usb']],
  ['OngeautoriseerdeToegang', [
    'sign-in', 'login', 'brute force', 'privilege', 'impossible travel', 'mfa',
    'vpn', 'lateral movement', 'mailbox access',
  ]],
]

function stelCategorieVoor(alertNaam) {
  const naam = alertNaam.toLowerCase()
  const match = CATEGORIE_TREFWOORDEN.find(([, woorden]) => woorden.some((woord) => naam.includes(woord)))
  return match ? match[0] : 'Overig'
}

// Maakt de beginwaarden van het ticketformulier op basis van een alert uit de alertlijst.
export function maakTicketVanAlert(alert) {
  const beschrijving = [
    `Aangemaakt vanuit alert: ${alert.type}`,
    `Ernst: ${alert.severity}`,
    `Getroffen asset: ${alert.target}`,
    `Gedetecteerd: ${alert.time}`,
    `Eigenaar alert: ${alert.owner}`,
    '',
    'Bevindingen:',
    '',
  ].join('\n')

  return {
    titel: alert.type.slice(0, TITEL_MAX_LENGTE),
    beschrijving,
    prioriteit: PRIORITEIT_PER_SEVERITY[alert.severity] ?? 'Midden',
    categorie: stelCategorieVoor(alert.type),
  }
}
