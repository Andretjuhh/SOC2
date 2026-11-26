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

// Kopregel in de beschrijving: alles erboven komt uit de alert en ligt vast, eronder schrijft de analist
export const BEVINDINGEN_KOP = 'Bevindingen:'

// Splitst een beschrijving in het vaste deel uit de alert en de vrije bevindingen.
// Zonder kopregel (bijvoorbeeld een handmatig ticket) is de hele tekst vrij.
export function splitsBeschrijving(beschrijving) {
  const regels = beschrijving.split('\n')
  const index = regels.findIndex((regel) => regel.trim() === BEVINDINGEN_KOP)
  if (index === -1) return { vast: null, vrij: beschrijving }
  return { vast: regels.slice(0, index + 1).join('\n'), vrij: regels.slice(index + 1).join('\n') }
}

export function voegBeschrijvingSamen(vast, vrij) {
  if (vast === null) return vrij
  return vrij.trim() ? `${vast}\n${vrij}` : vast
}

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

// Leeg formulier voor een handmatig ticket, bijvoorbeeld een melding van een klant zonder alert.
// Prioriteit en categorie zijn leeg, zodat de analist ze bewust moet kiezen.
export function leegTicket() {
  return { titel: '', beschrijving: '', prioriteit: '', categorie: '' }
}

// Maakt de beginwaarden van het ticketformulier op basis van een alert uit de alertlijst.
// De eigenaar staat er bewust niet in: die is op dit moment nog niet bekend, want de alert
// wordt pas na het opslaan geclaimd. Wie het ticket heeft geclaimd, staat bij het ticket zelf.
export function maakTicketVanAlert(alert) {
  const beschrijving = [
    `Aangemaakt vanuit alert: ${alert.type}`,
    `Ernst: ${alert.severity}`,
    `Getroffen asset: ${alert.target}`,
    `Gedetecteerd: ${alert.time}`,
    '',
    BEVINDINGEN_KOP,
    '',
  ].join('\n')

  return {
    titel: alert.type.slice(0, TITEL_MAX_LENGTE),
    beschrijving,
    prioriteit: PRIORITEIT_PER_SEVERITY[alert.severity] ?? 'Midden',
    categorie: stelCategorieVoor(alert.type),
  }
}
