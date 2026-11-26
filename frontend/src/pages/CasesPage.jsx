import { useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

import CaseDetailDialog from '../features/cases/CaseDetailDialog'
import useCases from '../features/cases/useCases'
import { CASE_STATUSSEN, caseStatusLabel } from '../features/cases/caseLabels'
import { PRIORITEIT_STIJL, formatDatumTijd } from '../features/tickets/ticketLabels'
import styles from '../features/tickets/TicketPage.module.css'

const ALLE = 'Alle'

function CasesPage() {
  const { cases, loading, error, herlaad } = useCases()
  const [statusFilter, setStatusFilter] = useState(ALLE)
  const [zoekterm, setZoekterm] = useState('')
  // De geopende case staat in de URL (?case=3), zodat de badge in een ticket er direct naartoe kan linken
  const [zoekParams, setZoekParams] = useSearchParams()
  const geselecteerdId = Number(zoekParams.get('case')) || null

  function selecteer(caseId) {
    setZoekParams(caseId ? { case: String(caseId) } : {})
  }

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

          <div className="table-scroll">
            <table className="alerts-table">
              <thead>
                <tr><th>CASE</th><th>TITEL</th><th>PRIORITEIT</th><th>STATUS</th><th>TICKETS</th><th>AANGEMAAKT</th></tr>
              </thead>
              <tbody>
                {!loading && !error && gefilterd.map((zaak) => (
                  <tr
                    key={zaak.caseID}
                    className={`${styles.rij}${zaak.caseID === geselecteerdId ? ` ${styles.actief}` : ''}`}
                    onClick={() => selecteer(zaak.caseID)}
                    onKeyDown={(event) => selecteerMetToets(event, zaak.caseID)}
                    tabIndex={0}
                    aria-selected={zaak.caseID === geselecteerdId}
                  >
                    <td className={styles.nummer}>{zaak.caseNummer}</td>
                    <td className="event-name-cell">
                      <span className={`event-marker ${PRIORITEIT_STIJL[zaak.prioriteit]}`} />
                      <strong>{zaak.titel}</strong>
                    </td>
                    <td>
                      <span className={`severity-badge ${PRIORITEIT_STIJL[zaak.prioriteit]}`}>
                        <i className={`severity-dot ${PRIORITEIT_STIJL[zaak.prioriteit]}-dot`} />{zaak.prioriteit}
                      </span>
                    </td>
                    <td><span className={`${styles.status} ${styles[`status${zaak.status}`]}`}>{caseStatusLabel(zaak.status)}</span></td>
                    <td>{zaak.aantalTickets}</td>
                    <td className="time-cell">{formatDatumTijd(zaak.aangemaaktOp)}</td>
                  </tr>
                ))}
                {loading && <tr><td className="empty-state" colSpan="6">Cases laden…</td></tr>}
                {!loading && error && <tr><td className="empty-state" colSpan="6">{error}</td></tr>}
                {!loading && !error && gefilterd.length === 0 && (
                  <tr>
                    <td className="empty-state" colSpan="6">
                      {cases.length === 0
                        ? 'Nog geen cases. Open een ticket en kies "Promoveer naar Case".'
                        : 'Geen cases gevonden met deze filters.'}
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
          <div className="table-footer"><span>{gefilterd.length} van {cases.length} cases</span></div>
        </section>

        {geselecteerdId && (
          <CaseDetailDialog
            key={geselecteerdId}
            caseId={geselecteerdId}
            onClose={() => selecteer(null)}
            onGewijzigd={herlaad}
          />
        )}
      </div>
    </section>
  )
}

export default CasesPage
