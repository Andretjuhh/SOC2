import { useMemo, useState } from 'react'

import TicketDetailPanel from '../features/tickets/TicketDetailPanel'
import useTickets from '../features/tickets/useTickets'
import {
  PRIORITEIT_STIJL, STATUSSEN, categorieLabel, formatDatumTijd, initialen, statusLabel,
} from '../features/tickets/ticketLabels'
import styles from '../features/tickets/TicketPage.module.css'

const ALLE = 'Alle'

function TicketPage() {
  const { tickets, loading, error, herlaad } = useTickets()
  const [statusFilter, setStatusFilter] = useState(ALLE)
  const [zoekterm, setZoekterm] = useState('')
  const [geselecteerdId, setGeselecteerdId] = useState(null)

  const aantalPerStatus = useMemo(() => {
    const aantallen = Object.fromEntries(STATUSSEN.map((status) => [status.value, 0]))
    tickets.forEach((ticket) => { aantallen[ticket.status] = (aantallen[ticket.status] ?? 0) + 1 })
    return aantallen
  }, [tickets])

  const gefilterd = useMemo(() => {
    const term = zoekterm.trim().toLowerCase()
    return tickets.filter((ticket) => {
      const statusKlopt = statusFilter === ALLE || ticket.status === statusFilter
      const zoekKlopt = !term || [ticket.ticketNummer, ticket.titel, ticket.aangemaaktDoorNaam, ticket.aangemaaktDoorGebruikersnaam, ticket.beschrijving]
        .join(' ').toLowerCase().includes(term)
      return statusKlopt && zoekKlopt
    })
  }, [tickets, statusFilter, zoekterm])

  const geselecteerd = tickets.find((ticket) => ticket.ticketID === geselecteerdId) ?? null

  function selecteerMetToets(event, ticketId) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault()
      setGeselecteerdId(ticketId)
    }
  }

  return (
    <section className={styles.pagina} id="ticket-page">
      <header className={styles.kop}>
        <div>
          <h1>Tickets</h1>
          <p>Tickets die vanuit alerts zijn geclaimd. Klik op een ticket voor alle details.</p>
        </div>
        <button type="button" className={styles.knop} onClick={herlaad} disabled={loading}>
          {loading ? 'Laden…' : 'Vernieuwen'}
        </button>
      </header>

      <div className={styles.indeling}>
        <section className={`panel ${styles.lijst}`}>
          <div className={styles.werkbalk}>
            <div className="alert-filters" aria-label="Filter tickets op status">
              {[{ value: ALLE, label: 'Alle' }, ...STATUSSEN].map((optie) => (
                <button
                  key={optie.value}
                  type="button"
                  className={`filter-chip${statusFilter === optie.value ? ' selected' : ''}`}
                  aria-pressed={statusFilter === optie.value}
                  onClick={() => setStatusFilter(optie.value)}
                >
                  {optie.label}
                  <span className={styles.aantal}>
                    {optie.value === ALLE ? tickets.length : aantalPerStatus[optie.value]}
                  </span>
                </button>
              ))}
            </div>
            <input
              className={styles.zoek}
              type="search"
              placeholder="Zoek op nummer, titel of analist"
              aria-label="Tickets zoeken"
              value={zoekterm}
              onChange={(event) => setZoekterm(event.target.value)}
            />
          </div>

          <div className="table-scroll">
            <table className="alerts-table">
              <thead>
                <tr><th>TICKET</th><th>TITEL</th><th>PRIORITEIT</th><th>CATEGORIE</th><th>STATUS</th><th>GECLAIMD DOOR</th><th>AANGEMAAKT</th></tr>
              </thead>
              <tbody>
                {!loading && !error && gefilterd.map((ticket) => (
                  <tr
                    key={ticket.ticketID}
                    className={`${styles.rij}${ticket.ticketID === geselecteerdId ? ` ${styles.actief}` : ''}`}
                    onClick={() => setGeselecteerdId(ticket.ticketID)}
                    onKeyDown={(event) => selecteerMetToets(event, ticket.ticketID)}
                    tabIndex={0}
                    aria-selected={ticket.ticketID === geselecteerdId}
                  >
                    <td className={styles.nummer}>{ticket.ticketNummer}</td>
                    <td className="event-name-cell">
                      <span className={`event-marker ${PRIORITEIT_STIJL[ticket.prioriteit]}`} />
                      <strong>{ticket.titel}</strong>
                    </td>
                    <td>
                      <span className={`severity-badge ${PRIORITEIT_STIJL[ticket.prioriteit]}`}>
                        <i className={`severity-dot ${PRIORITEIT_STIJL[ticket.prioriteit]}-dot`} />{ticket.prioriteit}
                      </span>
                    </td>
                    <td>{categorieLabel(ticket.categorie)}</td>
                    <td><span className={`${styles.status} ${styles[`status${ticket.status}`]}`}>{statusLabel(ticket.status)}</span></td>
                    <td>
                      <span className={styles.persoon}>
                        <span className={styles.avatar} aria-hidden="true">{initialen(ticket.aangemaaktDoorNaam)}</span>
                        <span className={styles.persoonNamen}>
                          {ticket.aangemaaktDoorNaam}
                          <span className={styles.gebruikersnaam}>@{ticket.aangemaaktDoorGebruikersnaam}</span>
                        </span>
                      </span>
                    </td>
                    <td className="time-cell">{formatDatumTijd(ticket.aangemaaktOp)}</td>
                  </tr>
                ))}
                {loading && <tr><td className="empty-state" colSpan="7">Tickets laden…</td></tr>}
                {!loading && error && <tr><td className="empty-state" colSpan="7">{error}</td></tr>}
                {!loading && !error && gefilterd.length === 0 && (
                  <tr>
                    <td className="empty-state" colSpan="7">
                      {tickets.length === 0
                        ? 'Nog geen tickets. Claim een alert op de Alerts-pagina om een ticket aan te maken.'
                        : 'Geen tickets gevonden met deze filters.'}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
          <div className="table-footer"><span>{gefilterd.length} van {tickets.length} tickets</span></div>
        </section>

        {geselecteerd && (
          <TicketDetailPanel
            key={geselecteerd.ticketID}
            ticket={geselecteerd}
            onClose={() => setGeselecteerdId(null)}
            onGewijzigd={herlaad}
          />
        )}
      </div>
    </section>
  )
}

export default TicketPage
