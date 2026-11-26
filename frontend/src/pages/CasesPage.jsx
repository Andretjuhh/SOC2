import { useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

import CaseDetailDialog from '../features/cases/CaseDetailDialog'
import useCases from '../features/cases/useCases'
import useTickets from '../features/tickets/useTickets'
import TicketDetailPanel from '../features/tickets/TicketDetailPanel'
import Werkruimte from '../features/cases/Werkruimte'
import { CASE_STATUSSEN, caseStatusLabel } from '../features/cases/caseLabels'
import { PRIORITEIT_STIJL, formatDatumTijd, statusLabel } from '../features/tickets/ticketLabels'
import styles from '../features/tickets/TicketPage.module.css'
import kaartStyles from '../features/cases/CasesPage.module.css'

const ALLE = 'Alle'
// Zoveel tickets tonen we per blok; de rest staat in het casedossier
const MAX_TICKETS_IN_KAART = 4

function CasesPage() {
  const { cases, loading: casesLaden, error: casesFout, herlaad: herlaadCases } = useCases()
  // De tickets per case halen we uit de ticketlijst: elk ticket weet zelf bij welke case het hoort
  const { tickets, loading: ticketsLaden, herlaad: herlaadTickets } = useTickets()
  const loading = casesLaden || ticketsLaden
  const error = casesFout
  const [statusFilter, setStatusFilter] = useState(ALLE)
  const [zoekterm, setZoekterm] = useState('')
  // De geopende case en het geopende ticket staan in de URL (?case=3&ticket=17),
  // zodat de badge in een ticket er direct naartoe kan linken
  const [zoekParams, setZoekParams] = useSearchParams()
  const geselecteerdId = Number(zoekParams.get('case')) || null
  const ticketId = Number(zoekParams.get('ticket')) || null
  // Telt op als een ticket is gewijzigd, zodat een open casedossier zijn ticketlijst ververst
  const [ticketWijzigingen, setTicketWijzigingen] = useState(0)

  function zetParam(naam, waarde) {
    setZoekParams((huidig) => {
      const nieuw = new URLSearchParams(huidig)
      if (waarde) nieuw.set(naam, String(waarde))
      else nieuw.delete(naam)
      return nieuw
    })
  }

  const selecteer = (caseId) => zetParam('case', caseId)
  const openTicket = (id) => zetParam('ticket', id)

  function herlaad() {
    herlaadCases()
    herlaadTickets()
  }

  function ticketGewijzigd() {
    herlaad()
    setTicketWijzigingen((teller) => teller + 1)
  }

  const geopendTicket = tickets.find((ticket) => ticket.ticketID === ticketId) ?? null

  const ticketsPerCase = useMemo(() => {
    const groepen = {}
    tickets.filter((ticket) => ticket.caseID).forEach((ticket) => {
      groepen[ticket.caseID] = [...(groepen[ticket.caseID] ?? []), ticket]
    })
    return groepen
  }, [tickets])

  const aantalPerStatus = useMemo(() => {
    const aantallen = Object.fromEntries(CASE_STATUSSEN.map((status) => [status.value, 0]))
    cases.forEach((zaak) => { aantallen[zaak.status] = (aantallen[zaak.status] ?? 0) + 1 })
    return aantallen
  }, [cases])

  const gefilterd = useMemo(() => {
    const term = zoekterm.trim().toLowerCase()
    return cases.filter((zaak) => {
      const statusKlopt = statusFilter === ALLE || zaak.status === statusFilter
      const zoekKlopt = !term || `${zaak.caseNummer} ${zaak.titel}`.toLowerCase().includes(term)
      return statusKlopt && zoekKlopt
    })
  }, [cases, statusFilter, zoekterm])

  function selecteerMetToets(event, caseId) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault()
      selecteer(caseId)
    }
  }

  return (
    <section className={styles.pagina} id="cases-page">
      <header className={styles.kop}>
        <div>
          <h1>Cases</h1>
          <p>Een case bundelt tickets die bij hetzelfde incident horen. Je maakt een case door een ticket te promoveren via het ticketvenster.</p>
        </div>
        <div className={styles.kopKnoppen}>
          <button type="button" className={styles.knop} onClick={herlaad} disabled={loading}>
            {loading ? 'Laden…' : 'Vernieuwen'}
          </button>
        </div>
      </header>

      <div className={styles.indeling}>
        <section className={`panel ${styles.lijst}`}>
          <div className={styles.werkbalk}>
            <div className="alert-filters" aria-label="Filter cases op status">
              {[{ value: ALLE, label: 'Alle' }, ...CASE_STATUSSEN].map((optie) => (
                <button
                  key={optie.value}
                  type="button"
                  className={`filter-chip${statusFilter === optie.value ? ' selected' : ''}`}
                  aria-pressed={statusFilter === optie.value}
                  onClick={() => setStatusFilter(optie.value)}
                >
                  {optie.label}
                  <span className={styles.aantal}>
                    {optie.value === ALLE ? cases.length : aantalPerStatus[optie.value]}
                  </span>
                </button>
              ))}
            </div>
            <input
              className={styles.zoek}
              type="search"
              placeholder="Zoek op nummer of titel"
              aria-label="Cases zoeken"
              value={zoekterm}
              onChange={(event) => setZoekterm(event.target.value)}
            />
          </div>

          {loading && <p className={kaartStyles.leeg}>Cases laden…</p>}
          {!loading && error && <p className={kaartStyles.leeg}>{error}</p>}
          {!loading && !error && gefilterd.length === 0 && (
            <p className={kaartStyles.leeg}>
              {cases.length === 0
                ? 'Nog geen cases. Open een ticket en kies "Promoveer naar Case".'
                : 'Geen cases gevonden met deze filters.'}
            </p>
          )}

          {!loading && !error && gefilterd.length > 0 && (
            <div className={kaartStyles.raster}>
              {gefilterd.map((zaak) => {
                const caseTickets = [...(ticketsPerCase[zaak.caseID] ?? [])].sort((a, b) => a.ticketID - b.ticketID)
                const zichtbaar = caseTickets.slice(0, MAX_TICKETS_IN_KAART)
                const verborgen = caseTickets.length - zichtbaar.length
                return (
                  <article
                    key={zaak.caseID}
                    className={`${kaartStyles.kaart} ${kaartStyles[PRIORITEIT_STIJL[zaak.prioriteit]] ?? ''}${zaak.caseID === geselecteerdId ? ` ${kaartStyles.kaartActief}` : ''}`}
                    onClick={() => selecteer(zaak.caseID)}
                    onKeyDown={(event) => selecteerMetToets(event, zaak.caseID)}
                    tabIndex={0}
                    aria-label={`${zaak.caseNummer} ${zaak.titel} openen`}
                  >
                    <div className={kaartStyles.kaartKop}>
                      <span className={styles.nummer}>{zaak.caseNummer}</span>
                      <span className={`${styles.status} ${styles[`status${zaak.status}`]}`}>{caseStatusLabel(zaak.status)}</span>
                    </div>
                    <h2 className={kaartStyles.kaartTitel}>{zaak.titel}</h2>
                    <p className={kaartStyles.kaartInfo}>
                      {zaak.prioriteit} · {zaak.aantalTickets} {zaak.aantalTickets === 1 ? 'ticket' : 'tickets'}
                    </p>

                    {zichtbaar.length > 0 && (
                      <ul className={kaartStyles.tickets}>
                        {zichtbaar.map((ticket) => (
                          <li key={ticket.ticketID}>
                            {/* Een ticket in het blok opent het ticketvenster hier, niet de case */}
                            <button
                              type="button"
                              className={kaartStyles.ticket}
                              onClick={(event) => { event.stopPropagation(); openTicket(ticket.ticketID) }}
                              onKeyDown={(event) => event.stopPropagation()}
                              title={ticket.titel}
                            >
                              <span className={styles.nummer}>{ticket.ticketNummer}</span>
                              <span className={kaartStyles.ticketTitel}>{ticket.titel}</span>
                              <span className={`${styles.status} ${styles[`status${ticket.status}`]} ${kaartStyles.ticketStatus}`}>
                                {statusLabel(ticket.status)}
                              </span>
                            </button>
                          </li>
                        ))}
                        {verborgen > 0 && <li className={kaartStyles.meer}>+{verborgen} meer</li>}
                      </ul>
                    )}

                    <span className={kaartStyles.kaartVoet}>{formatDatumTijd(zaak.aangemaaktOp)}</span>
                  </article>
                )
              })}
            </div>
          )}

          <div className="table-footer"><span>{gefilterd.length} van {cases.length} cases</span></div>
        </section>

        {/* Case en ticket openen in één werkruimte, naast elkaar */}
        {(geselecteerdId || geopendTicket) && (
          <Werkruimte
            aantal={(geselecteerdId ? 1 : 0) + (geopendTicket ? 1 : 0)}
            onEscape={() => (geopendTicket ? openTicket(null) : selecteer(null))}
            onSluitAlles={() => setZoekParams({})}
          >
            {geselecteerdId && (
              <CaseDetailDialog
                key={geselecteerdId}
                ingebed
                caseId={geselecteerdId}
                onClose={() => selecteer(null)}
                onGewijzigd={herlaad}
                onOpenTicket={openTicket}
                ververs={ticketWijzigingen}
              />
            )}
            {geopendTicket && (
              <TicketDetailPanel
                key={geopendTicket.ticketID}
                ingebed
                ticket={geopendTicket}
                onClose={() => openTicket(null)}
                onGewijzigd={ticketGewijzigd}
              />
            )}
          </Werkruimte>
        )}
      </div>
    </section>
  )
}

export default CasesPage
