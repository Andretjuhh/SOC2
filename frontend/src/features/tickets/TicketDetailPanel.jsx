import { useEffect, useRef, useState } from 'react'

import { fetchTicketHistorie } from './api/ticketsApi'
import TicketBewerken from './TicketBewerken'
import { PRIORITEIT_STIJL, categorieLabel, formatDatumTijd, initialen, statusLabel } from './ticketLabels'
import styles from './TicketPage.module.css'

function useTicketHistorie(ticketId, laatstGewijzigdOp) {
  const [historie, setHistorie] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    fetchTicketHistorie(ticketId, controller.signal)
      .then((data) => { setHistorie(data); setError('') })
      .catch((requestError) => { if (requestError.name !== 'AbortError') setError(requestError.message) })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })

    return () => controller.abort()
  }, [ticketId, laatstGewijzigdOp])

  return { historie, loading, error }
}

function Gegeven({ label, children }) {
  return (
    <div className={styles.gegeven}>
      <dt>{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}

function TicketDetailPanel({ ticket, onClose, onGewijzigd }) {
  const { historie, loading, error } = useTicketHistorie(ticket.ticketID, ticket.laatstGewijzigdOp)

  const dialogRef = useRef(null)

  // Native <dialog>, net als het ticketformulier bij Claim: focusbeheer, Escape en een backdrop
  useEffect(() => {
    const dialog = dialogRef.current
    if (dialog && !dialog.open) dialog.showModal()
    return () => dialog?.close()
  }, [])

  return (
    <dialog
      ref={dialogRef}
      className={styles.detail}
      aria-labelledby="ticket-detail-titel"
      onCancel={(event) => { event.preventDefault(); onClose() }}
      onClick={(event) => { if (event.target === dialogRef.current) onClose() }}
    >
      <div className={styles.detailInhoud}>
      <header className={styles.detailHeader}>
        <div>
          <span className={styles.nummer}>{ticket.ticketNummer}</span>
          <h2 id="ticket-detail-titel">{ticket.titel}</h2>
        </div>
        <button type="button" className={styles.sluit} onClick={onClose} aria-label="Details sluiten">×</button>
      </header>

      <div className={styles.badges}>
        <span className={`${styles.status} ${styles[`status${ticket.status}`]}`}>{statusLabel(ticket.status)}</span>
        <span className={`severity-badge ${PRIORITEIT_STIJL[ticket.prioriteit]}`}>
          <i className={`severity-dot ${PRIORITEIT_STIJL[ticket.prioriteit]}-dot`} />{ticket.prioriteit}
        </span>
        <span className={styles.categorie}>{categorieLabel(ticket.categorie)}</span>
      </div>

      <dl className={styles.gegevens}>
        <Gegeven label="Geclaimd door">
          <span className={styles.persoon}>
            <span className={styles.avatar} aria-hidden="true">{initialen(ticket.aangemaaktDoorNaam)}</span>
            <span className={styles.persoonNamen}>
              {ticket.aangemaaktDoorNaam}
              <span className={styles.gebruikersnaam}>@{ticket.aangemaaktDoorGebruikersnaam}</span>
            </span>
          </span>
        </Gegeven>
        <Gegeven label="Aangemaakt">{formatDatumTijd(ticket.aangemaaktOp)}</Gegeven>
        <Gegeven label="Laatst gewijzigd">{formatDatumTijd(ticket.laatstGewijzigdOp)}</Gegeven>
        <Gegeven label="Afgerond">{formatDatumTijd(ticket.afgerondOp)}</Gegeven>
      </dl>

      {/* Bij een open ticket staat de beschrijving in het bewerkformulier hieronder */}
      {ticket.status === 'Afgerond' && (
        <section className={styles.sectie}>
          <h3>Beschrijving</h3>
          <p className={styles.tekst}>{ticket.beschrijving}</p>
        </section>
      )}

      {ticket.oplossing && (
        <section className={styles.sectie}>
          <h3>Oplossing</h3>
          <p className={styles.tekst}>{ticket.oplossing}</p>
        </section>
      )}

      <TicketBewerken ticket={ticket} onGewijzigd={onGewijzigd} />

      <section className={styles.sectie}>
        <h3>Historie</h3>
        {loading && <p className={styles.leeg}>Historie laden…</p>}
        {!loading && error && <p className={styles.leeg}>{error}</p>}
        {!loading && !error && historie.length === 0 && <p className={styles.leeg}>Nog geen historie.</p>}
        {!loading && !error && historie.length > 0 && (
          <ol className={styles.tijdlijn}>
            {historie.map((regel, index) => (
              <li key={`${regel.tijdstip}-${index}`}>
                <div className={styles.tijdlijnKop}>
                  <strong>{regel.actie}</strong>
                  <time dateTime={regel.tijdstip}>{formatDatumTijd(regel.tijdstip)}</time>
                </div>
                <p>{regel.omschrijving}</p>
                <span className={styles.door}>
                  door {regel.gebruikerNaam ?? 'onbekend'}
                  {regel.gebruikersnaam && <span className={styles.gebruikersnaam}> @{regel.gebruikersnaam}</span>}
                </span>
              </li>
            ))}
          </ol>
        )}
      </section>
      </div>
    </dialog>
  )
}

export default TicketDetailPanel
