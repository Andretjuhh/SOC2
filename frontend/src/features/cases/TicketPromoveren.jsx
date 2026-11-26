import { useState } from 'react'

import { fetchCases, promoveerTicket } from './api/casesApi'
import { caseStatusLabel } from './caseLabels'
import { PRIORITEITEN } from '../tickets/ticketOptions'
import styles from '../tickets/TicketPage.module.css'

// Onderdeel van het ticketvenster: het ticket promoveren naar een nieuwe of bestaande case.
function TicketPromoveren({ ticket, versie, onGepromoveerd, onConflict }) {
  const [open, setOpen] = useState(false)
  const [soort, setSoort] = useState('nieuw')
  const [formulier, setFormulier] = useState({ titel: ticket.titel, beschrijving: '', prioriteit: ticket.prioriteit })
  const [caseId, setCaseId] = useState('')
  const [cases, setCases] = useState(null)
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')

  async function kiesSoort(nieuweSoort) {
    setSoort(nieuweSoort)
    setFout('')
    // Open cases pas ophalen als de analist voor een bestaande case kiest
    if (nieuweSoort === 'bestaand' && cases === null) {
      try {
        const alle = await fetchCases()
        setCases(alle.filter((zaak) => zaak.status !== 'Afgesloten'))
      } catch (error) {
        setFout(error.message)
        setCases([])
      }
    }
  }

  function wijzig(veld, waarde) {
    setFormulier((huidig) => ({ ...huidig, [veld]: waarde }))
    setFout('')
  }

  function annuleren() {
    setOpen(false)
    setFout('')
  }

  async function promoveren(event) {
    event.preventDefault()
    let gegevens
    if (soort === 'nieuw') {
      if (!formulier.titel.trim() || !formulier.beschrijving.trim() || !formulier.prioriteit) {
        setFout('Vul alle verplichte velden in: titel, beschrijving en prioriteit.')
        return
      }
      gegevens = { titel: formulier.titel.trim(), beschrijving: formulier.beschrijving.trim(), prioriteit: formulier.prioriteit }
    } else {
      if (!caseId) {
        setFout('Kies een case om het ticket aan toe te voegen.')
        return
      }
      gegevens = { caseID: Number(caseId) }
    }

    setBezig(true)
    setFout('')
    try {
      const zaak = await promoveerTicket(ticket.ticketID, gegevens, versie)
      onGepromoveerd(zaak)
    } catch (error) {
      setFout(error.message)
      if (error.status === 409) onConflict()
    } finally {
      setBezig(false)
    }
  }

  if (!open) {
    return (
      <section className={styles.sectie}>
        <h3>Escaleren</h3>
        <p className={styles.leeg}>Hoort dit ticket bij een groter incident? Promoveer het naar een case.</p>
        <button type="button" className={`${styles.knop} ${styles.knopPromoveer}`} onClick={() => setOpen(true)} disabled={!versie}>
          Promoveer naar Case
        </button>
      </section>
    )
  }

  return (
    <form className={`${styles.sectie} ${styles.formulier}`} onSubmit={promoveren}>
      <h3>Promoveer naar Case</h3>

      <fieldset className={styles.keuze}>
        <legend className={styles.verborgen}>Soort case</legend>
        <label>
          <input type="radio" name="soort" checked={soort === 'nieuw'} onChange={() => kiesSoort('nieuw')} disabled={bezig} />
          Nieuwe case
        </label>
        <label>
          <input type="radio" name="soort" checked={soort === 'bestaand'} onChange={() => kiesSoort('bestaand')} disabled={bezig} />
          Toevoegen aan bestaande case
        </label>
      </fieldset>

      {soort === 'nieuw' && (
        <>
          <label className={styles.veld}>
            <span>Titel van de case</span>
            <input value={formulier.titel} onChange={(event) => wijzig('titel', event.target.value)} maxLength={200} disabled={bezig} />
          </label>
          <label className={styles.veld}>
            <span>Beschrijving van de case</span>
            <textarea
              rows={4}
              value={formulier.beschrijving}
              onChange={(event) => wijzig('beschrijving', event.target.value)}
              placeholder="Waarom is dit een groter incident?"
              disabled={bezig}
            />
          </label>
          <label className={styles.veld}>
            <span>Prioriteit van de case</span>
            <select value={formulier.prioriteit} onChange={(event) => wijzig('prioriteit', event.target.value)} disabled={bezig}>
              {PRIORITEITEN.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
            </select>
          </label>
        </>
      )}

      {soort === 'bestaand' && (
        cases === null ? <p className={styles.leeg}>Cases laden…</p>
          : cases.length === 0 ? <p className={styles.leeg}>Er zijn geen open cases. Maak een nieuwe case aan.</p>
            : (
              <label className={styles.veld}>
                <span>Case</span>
                <select value={caseId} onChange={(event) => { setCaseId(event.target.value); setFout('') }} disabled={bezig}>
                  <option value="">Kies…</option>
                  {cases.map((zaak) => (
                    <option key={zaak.caseID} value={zaak.caseID}>
                      {zaak.caseNummer} · {zaak.titel} ({caseStatusLabel(zaak.status)})
                    </option>
                  ))}
                </select>
              </label>
            )
      )}

      <div className={styles.knoppenRij}>
        <button type="submit" className={`${styles.knop} ${styles.knopPromoveer}`} disabled={bezig || !versie}>
          {bezig ? 'Bezig…' : 'Promoveren'}
        </button>
        <button type="button" className={styles.knop} onClick={annuleren} disabled={bezig}>Annuleren</button>
      </div>

      {fout && <p className={styles.fout} role="alert">{fout}</p>}
    </form>
  )
}

export default TicketPromoveren
