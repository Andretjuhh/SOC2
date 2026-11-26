import { useEffect, useRef, useState } from 'react'

import { createTicket } from './api/createTicket'
import {
  CATEGORIEEN, PRIORITEITEN, TITEL_MAX_LENGTE, leegTicket, maakTicketVanAlert, splitsBeschrijving, voegBeschrijvingSamen,
} from './ticketOptions'
import styles from './CreateTicketDialog.module.css'

// Met een alert: het formulier bij Claim (de hoofdroute). Zonder alert: een handmatig ticket.
function CreateTicketDialog({ alert = null, onClose, onCreated }) {
  const handmatig = alert === null
  const dialogRef = useRef(null)
  // Bij een alert ligt alles boven "Bevindingen:" vast; het tekstveld bevat alleen de bevindingen
  const [vasteTekst] = useState(() => (handmatig ? null : splitsBeschrijving(maakTicketVanAlert(alert).beschrijving).vast))
  const [formulier, setFormulier] = useState(() => (
    handmatig ? leegTicket() : { ...maakTicketVanAlert(alert), beschrijving: '' }
  ))
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')

  // Native <dialog>: geeft focusbeheer, Escape en een backdrop zonder extra code.
  useEffect(() => {
    const dialog = dialogRef.current
    if (dialog && !dialog.open) dialog.showModal()
    return () => dialog?.close()
  }, [])

  function wijzig(veld) {
    return (event) => setFormulier((huidig) => ({ ...huidig, [veld]: event.target.value }))
  }

  function sluit() {
    if (!bezig) onClose()
  }

  async function handleSubmit(event) {
    event.preventDefault()
    if (bezig) return

    const beschrijving = voegBeschrijvingSamen(vasteTekst, formulier.beschrijving)

    if (!formulier.titel.trim() || !beschrijving.trim() || !formulier.prioriteit || !formulier.categorie) {
      setFout('Vul alle verplichte velden in: titel, beschrijving, prioriteit en categorie.')
      return
    }

    setBezig(true)
    setFout('')

    try {
      const alertID = Number.isInteger(alert?.id) ? alert.id : null
      const ticket = await createTicket({ ...formulier, beschrijving, alertID })
      onCreated(ticket)
    } catch (error) {
      setFout(error.message)
      setBezig(false)
    }
  }

  return (
    <dialog
      ref={dialogRef}
      className={styles.dialog}
      aria-labelledby="ticket-dialog-titel"
      onCancel={(event) => { event.preventDefault(); sluit() }}
      onClick={(event) => { if (event.target === dialogRef.current) sluit() }}
    >
      <form className={styles.form} onSubmit={handleSubmit} aria-busy={bezig}>
        <header className={styles.header}>
          <div>
            <h2 id="ticket-dialog-titel">{handmatig ? 'Nieuw ticket' : 'Ticket aanmaken'}</h2>
            {handmatig
              ? <p>Voor een melding zonder alert, bijvoorbeeld van een klant. Een ticket vanuit een alert maak je via Claim op de Alerts-pagina.</p>
              : <p>Vanuit alert <strong>{alert.type}</strong> op <code>{alert.target}</code></p>}
          </div>
          <button type="button" className={styles.close} onClick={sluit} aria-label="Sluiten" disabled={bezig}>×</button>
        </header>

        <fieldset className={styles.fields} disabled={bezig}>
          <label className={styles.field}>
            <span>Titel</span>
            <input
              value={formulier.titel}
              onChange={wijzig('titel')}
              maxLength={TITEL_MAX_LENGTE}
              required
              autoFocus
            />
          </label>

          <div className={styles.row}>
            <label className={styles.field}>
              <span>Prioriteit</span>
              <select value={formulier.prioriteit} onChange={wijzig('prioriteit')} required>
                {handmatig && <option value="" disabled>Kies…</option>}
                {PRIORITEITEN.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
              </select>
            </label>
            <label className={styles.field}>
              <span>Categorie</span>
              <select value={formulier.categorie} onChange={wijzig('categorie')} required>
                {handmatig && <option value="" disabled>Kies…</option>}
                {CATEGORIEEN.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
              </select>
            </label>
          </div>

          {handmatig ? (
            <label className={styles.field}>
              <span>Beschrijving</span>
              <textarea value={formulier.beschrijving} onChange={wijzig('beschrijving')} rows={9} required />
            </label>
          ) : (
            <>
              <div className={styles.field}>
                <span>Gegevens uit de alert (niet aan te passen)</span>
                <pre className={styles.vasteTekst}>{vasteTekst.split('\n').slice(0, -1).join('\n').trimEnd()}</pre>
              </div>
              <label className={styles.field}>
                <span>Bevindingen</span>
                <textarea
                  value={formulier.beschrijving}
                  onChange={wijzig('beschrijving')}
                  rows={5}
                  placeholder="Wat heb je onderzocht en gevonden?"
                />
              </label>
            </>
          )}
        </fieldset>

        {fout && <p className={styles.error} role="alert">{fout}</p>}

        <footer className={styles.footer}>
          <span className={styles.hint}>
            {handmatig
              ? <>Status wordt automatisch <strong>Open</strong>.</>
              : <>Status wordt <strong>Open</strong>, of <strong>In behandeling</strong> als je al bevindingen invult.</>}
          </span>
          <button type="button" className={styles.secondary} onClick={sluit} disabled={bezig}>Annuleren</button>
          <button type="submit" className={styles.primary} disabled={bezig}>
            {bezig ? 'Bezig...' : handmatig ? 'Opslaan' : 'Claim & ticket aanmaken'}
          </button>
        </footer>
      </form>
    </dialog>
  )
}

export default CreateTicketDialog
