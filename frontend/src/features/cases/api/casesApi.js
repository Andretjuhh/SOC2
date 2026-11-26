import { getJson, sendJson } from '../../tickets/api/ticketsApi'

// GET /api/cases: alle cases van de eigen klant, nieuwste eerst
export function fetchCases(signal) {
  return getJson('/api/cases', signal)
}

// GET /api/cases/{id}: de case met alle gekoppelde tickets en de versie voor gelijktijdig bewerken
export function fetchCase(caseId, signal) {
  return getJson(`/api/cases/${caseId}`, signal)
}

// PATCH /api/cases/{id}: alleen de velden die je meestuurt worden aangepast
export function updateCase(caseId, wijzigingen, versie) {
  return sendJson(`/api/cases/${caseId}`, 'PATCH', { ...wijzigingen, versie })
}

// POST /api/tickets/{id}/promoveren
// Met caseID: ticket toevoegen aan een bestaande case. Zonder: nieuwe case met titel, beschrijving en prioriteit.
// De versie is die van het ticket.
export function promoveerTicket(ticketId, gegevens, versie) {
  return sendJson(`/api/tickets/${ticketId}/promoveren`, 'POST', { ...gegevens, versie })
}

// POST /api/cases/{id}/tijdlijn: gebeurtenis toevoegen. Tijdstip = wanneer het gebeurde (ISO, UTC).
export function voegTijdlijnItemToe(caseId, item) {
  return sendJson(`/api/cases/${caseId}/tijdlijn`, 'POST', item)
}

// DELETE /api/cases/{id}/tijdlijn/{itemId}: een verkeerd ingevoerde gebeurtenis verwijderen
export function verwijderTijdlijnItem(caseId, itemId) {
  return sendJson(`/api/cases/${caseId}/tijdlijn/${itemId}`, 'DELETE')
}
