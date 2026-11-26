import { TicketApiError } from './createTicket'

export function meldingVoorStatus(status, data) {
  if (status === 400 && data?.errors) return Object.values(data.errors).flat().join(' ')
  if (status === 401) return 'Je bent niet ingelogd. Log in om tickets te bekijken.'
  if (status === 403) return data?.title ?? 'Je hebt geen rechten voor deze actie.'
  if (status === 404) return data?.melding ?? 'Ticket niet gevonden.'
  if (status === 409) return data?.title ?? 'Het ticket is intussen gewijzigd. Ververs en probeer het opnieuw.'
  return data?.title ?? `Het verzoek is mislukt (${status}).`
}

export async function getJson(url, signal) {
  let response

  try {
    response = await fetch(url, { credentials: 'include', signal })
  } catch (error) {
    if (error.name === 'AbortError') throw error
    throw new TicketApiError('De server is niet bereikbaar. Probeer het later opnieuw.', 0)
  }

  const data = await response.json().catch(() => null)
  if (!response.ok) throw new TicketApiError(meldingVoorStatus(response.status, data), response.status)
  return data
}

// Voor verzoeken die iets wijzigen (PATCH, POST) met een JSON-body
export async function sendJson(url, method, body) {
  let response

  try {
    response = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(body),
    })
  } catch {
    throw new TicketApiError('De server is niet bereikbaar. Probeer het later opnieuw.', 0)
  }

  const data = await response.json().catch(() => null)
  if (!response.ok) throw new TicketApiError(meldingVoorStatus(response.status, data), response.status)
  return data
}

// GET /api/tickets: alle tickets van de eigen klant, nieuwste eerst
export function fetchTickets(signal) {
  return getJson('/api/tickets', signal)
}

// GET /api/tickets/{id}: het volledige ticket, inclusief de versie voor gelijktijdig bewerken
export function fetchTicket(ticketId, signal) {
  return getJson(`/api/tickets/${ticketId}`, signal)
}

// GET /api/tickets/{id}/historie: alle acties op een ticket, oudste eerst
export function fetchTicketHistorie(ticketId, signal) {
  return getJson(`/api/tickets/${ticketId}/historie`, signal)
}

// PATCH /api/tickets/{id}: alleen de velden die je meestuurt worden aangepast.
// De versie is verplicht; klopt die niet meer, dan geeft de API 409 Conflict.
export function updateTicket(ticketId, wijzigingen, versie) {
  return sendJson(`/api/tickets/${ticketId}`, 'PATCH', { ...wijzigingen, versie })
}

// POST /api/tickets/{id}/afronden: een oplossing is verplicht
export function rondTicketAf(ticketId, oplossing, versie) {
  return sendJson(`/api/tickets/${ticketId}/afronden`, 'POST', { oplossing: oplossing.trim(), versie })
}
