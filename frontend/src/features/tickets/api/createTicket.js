export class TicketApiError extends Error {
  constructor(message, status) {
    super(message)
    this.name = 'TicketApiError'
    this.status = status
  }
}

function meldingVoorStatus(status, data) {
  if (status === 400 && data?.errors) {
    return Object.values(data.errors).flat().join(' ')
  }
  if (status === 401) {
    return 'Je bent niet ingelogd. Log in om een ticket aan te maken.'
  }
  if (status === 403) {
    return data?.title ?? 'Je hebt geen rechten om tickets aan te maken.'
  }
  if (status === 404) {
    return data?.melding ?? 'De alert bestaat niet (meer).'
  }
  return data?.title ?? `Ticket aanmaken is mislukt (${status}).`
}

// POST /api/tickets. Geeft het aangemaakte ticket (TicketResponse) terug.
export async function createTicket(ticket) {
  let response

  try {
    response = await fetch('/api/tickets', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({
        titel: ticket.titel.trim(),
        beschrijving: ticket.beschrijving.trim(),
        prioriteit: ticket.prioriteit,
        categorie: ticket.categorie,
        // Koppeling met de alert waaruit het ticket ontstaat (leeg bij een handmatig ticket)
        alertID: ticket.alertID ?? null,
      }),
    })
  } catch {
    throw new TicketApiError('De server is niet bereikbaar. Probeer het later opnieuw.', 0)
  }

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    throw new TicketApiError(meldingVoorStatus(response.status, data), response.status)
  }

  return data
}
