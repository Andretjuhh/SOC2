import { TicketApiError } from './createTicket'

function meldingVoorStatus(status, data) {
  if (status === 401) return 'Je bent niet ingelogd. Log in om tickets te bekijken.'
  if (status === 403) return data?.title ?? 'Je hebt geen rechten om tickets te bekijken.'
  if (status === 404) return data?.melding ?? 'Ticket niet gevonden.'
  return data?.title ?? `Tickets laden is mislukt (${status}).`
}

async function getJson(url, signal) {
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

// GET /api/tickets: alle tickets van de eigen klant, nieuwste eerst
export function fetchTickets(signal) {
  return getJson('/api/tickets', signal)
}

// GET /api/tickets/{id}/historie: alle acties op een ticket, oudste eerst
export function fetchTicketHistorie(ticketId, signal) {
  return getJson(`/api/tickets/${ticketId}/historie`, signal)
}
