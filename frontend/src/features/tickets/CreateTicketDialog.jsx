import { useEffect, useRef, useState } from 'react'

import { createTicket } from './api/createTicket'
import { CATEGORIEEN, PRIORITEITEN, TITEL_MAX_LENGTE, maakTicketVanAlert } from './ticketOptions'
import styles from './CreateTicketDialog.module.css'

function CreateTicketDialog({ alert, onClose, onCreated }) {
  const dialogRef = useRef(null)
  const [formulier, setFormulier] = useState(() => maakTicketVanAlert(alert))
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

    if (!formulier.titel.trim() || !formulier.beschrijving.trim()) {
      setFout('Vul een titel en beschrijving in.')
      return
    }

    setBezig(true)
    setFout('')

    try {
      const ticket = await createTicket(formulier)
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
            <h2 id="ticket-dialog-titel">Ticket aanmaken</h2>
            <p>Vanuit alert <strong>{alert.type}</strong> op <code>{alert.target}</code></p>
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
              <select value={formulier.prioriteit} onChange={wijzig('prioriteit')}>
                {PRIORITEITEN.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
              </select>
            </label>
            <label className={styles.field}>
              <span>Categorie</span>
              <select value={formulier.categorie} onChange={wijzig('categorie')}>
                {CATEGORIEEN.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
              </select>
            </label>
          </div>

          <label className={styles.field}>
            <span>Beschrijving</span>
            <textarea value={formulier.beschrijving} onChange={wijzig('beschrijving')} rows={9} required />
          </label>
        </fieldset>

        {fout && <p className={styles.error} role="alert">{fout}</p>}

        <footer className={styles.footer}>
          <span className={styles.hint}>Status wordt automatisch <strong>Open</strong>.</span>
          <button type="button" className={styles.secondary} onClick={sluit} disabled={bezig}>Annuleren</button>
          <button type="submit" className={styles.primary} disabled={bezig}>
            {bezig ? 'Bezig...' : 'Claim & ticket aanmaken'}
          </button>
        </footer>
      </form>
    </dialog>
  )
}

export default CreateTicketDialog
