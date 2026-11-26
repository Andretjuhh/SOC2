import { useState } from 'react'

import { verwijderTijdlijnItem, voegTijdlijnItemToe } from './api/casesApi'
import { AANVALSFASES, faseLabel } from './caseLabels'
import { formatDatumTijd } from '../tickets/ticketLabels'
import styles from '../tickets/TicketPage.module.css'
import tijdlijnStyles from './AanvalsTijdlijn.module.css'

const LEEG_ITEM = { tijdstip: '', beschrijving: '', systeem: '', fase: '' }

// Huidige tijd als waarde voor <input type="datetime-local"> (lokale tijd, zonder seconden)
function nuLokaal() {
  const nu = new Date()
  nu.setMinutes(nu.getMinutes() - nu.getTimezoneOffset())
  return nu.toISOString().slice(0, 16)
}

// Bijv. "2 d 3 u 15 min"
function formatDuur(ms) {
  const minuten = Math.round(ms / 60000)
  const dagen = Math.floor(minuten / 1440)
  const uren = Math.floor((minuten % 1440) / 60)
  const rest = minuten % 60
  return [dagen && `${dagen} d`, uren && `${uren} u`, `${rest} min`].filter(Boolean).join(' ')
}

// Verblijftijd (dwell time): hoelang de aanvaller binnen was vóór de eerste alert
function berekenVerblijftijd(tijdlijn) {
  const eerste = tijdlijn[0]
  const detectie = tijdlijn.find((item) => item.bron === 'Alert')
  if (!eerste || !detectie) return null
  const verschil = new Date(detectie.tijdstip) - new Date(eerste.tijdstip)
  return verschil > 0 ? formatDuur(verschil) : null
}

// De aanvalstijdlijn van een case: wat deed de aanvaller en wanneer gebeurde dat.
// Alerts van gekoppelde tickets staan er automatisch in; de analist vult aan met eigen onderzoek.
function AanvalsTijdlijn({ zaak, afgesloten, onBijgewerkt }) {
  const [formOpen, setFormOpen] = useState(false)
  const [item, setItem] = useState(LEEG_ITEM)
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')
  const [teVerwijderen, setTeVerwijderen] = useState(null)

  const verblijftijd = berekenVerblijftijd(zaak.tijdlijn)

  function wijzig(veld, waarde) {
    setItem((huidig) => ({ ...huidig, [veld]: waarde }))
    setFout('')
  }

  async function voerUit(actie) {
    setBezig(true)
    setFout('')
    try {
      onBijgewerkt(await actie())
      return true
    } catch (error) {
      setFout(error.message)
      return false
    } finally {
      setBezig(false)
    }
  }

  async function toevoegen(event) {
    event.preventDefault()
    if (!item.tijdstip || !item.beschrijving.trim()) {
      setFout('Vul in wanneer het gebeurde en wat er gebeurde.')
      return
    }
    const tijdstip = new Date(item.tijdstip)
    if (tijdstip > new Date()) {
      setFout('Een gebeurtenis kan niet in de toekomst liggen.')
      return
    }

    const gelukt = await voerUit(() => voegTijdlijnItemToe(zaak.caseID, {
      tijdstip: tijdstip.toISOString(),
      beschrijving: item.beschrijving.trim(),
      systeem: item.systeem.trim() || null,
      fase: item.fase || null,
    }))
    if (gelukt) {
      setItem(LEEG_ITEM)
      setFormOpen(false)
    }
  }

  async function verwijderen(itemId) {
    // Eerste klik vraagt om bevestiging, tweede klik verwijdert
    if (teVerwijderen !== itemId) {
      setTeVerwijderen(itemId)
      return
    }
    setTeVerwijderen(null)
    await voerUit(() => verwijderTijdlijnItem(zaak.caseID, itemId))
  }

  return (
    <section className={styles.sectie}>
      <div className={tijdlijnStyles.kop}>
        <h3>Aanvalstijdlijn</h3>
        {!afgesloten && !formOpen && (
          <button type="button" className={styles.knop} onClick={() => setFormOpen(true)}>+ Gebeurtenis toevoegen</button>
        )}
      </div>
      <p className={styles.leeg}>
        Wat de aanvaller deed, op het moment dat het gebeurde. Alerts van gekoppelde tickets staan er automatisch in.
      </p>

      {verblijftijd && (
        <p className={tijdlijnStyles.verblijftijd}>
          Verblijftijd vóór detectie: <strong>{verblijftijd}</strong>
        </p>
      )}

      {formOpen && (
        <form className={`${styles.formulier} ${tijdlijnStyles.formulier}`} onSubmit={toevoegen}>
          <div className={tijdlijnStyles.rij}>
            <label className={styles.veld}>
              <span>Wanneer gebeurde het?</span>
              <input
                type="datetime-local"
                value={item.tijdstip}
                max={nuLokaal()}
                onChange={(event) => wijzig('tijdstip', event.target.value)}
                disabled={bezig}
              />
            </label>
            <label className={styles.veld}>
              <span>Fase (optioneel)</span>
              <select value={item.fase} onChange={(event) => wijzig('fase', event.target.value)} disabled={bezig}>
                <option value="">Onbekend</option>
                {AANVALSFASES.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
              </select>
            </label>
          </div>
          <label className={styles.veld}>
            <span>Wat gebeurde er?</span>
            <textarea
              rows={3}
              value={item.beschrijving}
              onChange={(event) => wijzig('beschrijving', event.target.value)}
              placeholder="Gebruiker klikt op link in phishingmail"
              maxLength={1000}
              disabled={bezig}
            />
          </label>
          <label className={styles.veld}>
            <span>Getroffen asset (optioneel)</span>
            <input
              value={item.systeem}
              onChange={(event) => wijzig('systeem', event.target.value)}
              placeholder="WS-114 of j.jansen@acme.io"
              maxLength={200}
              disabled={bezig}
            />
          </label>
          <div className={styles.knoppenRij}>
            <button type="submit" className={styles.knop} disabled={bezig}>{bezig ? 'Bezig…' : 'Toevoegen'}</button>
            <button type="button" className={styles.knop} onClick={() => { setFormOpen(false); setFout('') }} disabled={bezig}>
              Annuleren
            </button>
          </div>
        </form>
      )}

      {fout && <p className={styles.fout} role="alert">{fout}</p>}

      {zaak.tijdlijn.length === 0 ? (
        <p className={styles.leeg}>Nog geen gebeurtenissen.</p>
      ) : (
        <ol className={tijdlijnStyles.lijst}>
          {zaak.tijdlijn.map((regel, index) => (
            <li
              key={regel.itemID ?? `alert-${regel.ticketNummer}-${index}`}
              className={`${tijdlijnStyles.regel} ${regel.bron === 'Alert' ? tijdlijnStyles.regelAlert : ''}`}
            >
              <div className={tijdlijnStyles.regelKop}>
                <time dateTime={regel.tijdstip}>{formatDatumTijd(regel.tijdstip)}</time>
                {regel.bron === 'Alert' && <span className={tijdlijnStyles.chipAlert}>Alert</span>}
                {regel.fase && <span className={tijdlijnStyles.chip}>{faseLabel(regel.fase)}</span>}
                {regel.itemID && !afgesloten && (
                  <button
                    type="button"
                    className={tijdlijnStyles.verwijder}
                    onClick={() => verwijderen(regel.itemID)}
                    onBlur={() => setTeVerwijderen(null)}
                    disabled={bezig}
                  >
                    {teVerwijderen === regel.itemID ? 'Zeker weten?' : 'Verwijderen'}
                  </button>
                )}
              </div>
              <p className={tijdlijnStyles.beschrijving}>{regel.beschrijving}</p>
              <span className={tijdlijnStyles.bron}>
                {regel.systeem && <>Getroffen asset: {regel.systeem} · </>}
                {regel.bron === 'Alert' ? `uit ${regel.ticketNummer}` : `toegevoegd door ${regel.toegevoegdDoorNaam}`}
              </span>
            </li>
          ))}
        </ol>
      )}
    </section>
  )
}

export default AanvalsTijdlijn
