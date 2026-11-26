import { useEffect, useState } from 'react'

import { fetchCase, updateCase } from './api/casesApi'
import AanvalsTijdlijn from './AanvalsTijdlijn'
import Venster from '../tickets/Venster'
import { CASE_STATUSSEN, caseStatusLabel } from './caseLabels'
import { PRIORITEIT_STIJL, formatDatumTijd, statusLabel } from '../tickets/ticketLabels'
import styles from '../tickets/TicketPage.module.css'

const DOSSIERVELDEN = ['status', 'rootCause']

function useCase(caseId, ververs) {
  const [zaak, setZaak] = useState(null)
  const [fout, setFout] = useState('')
  const [herlaadTeller, setHerlaadTeller] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    fetchCase(caseId, controller.signal)
      .then((data) => { setZaak(data); setFout('') })
      .catch((error) => { if (error.name !== 'AbortError') setFout(error.message) })

    return () => controller.abort()
  }, [caseId, herlaadTeller, ververs])

  return { zaak, setZaak, fout, herlaad: () => setHerlaadTeller((teller) => teller + 1) }
}

function Gegeven({ label, children }) {
  return (
    <div className={styles.gegeven}>
      <dt>{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}

function CaseDetailDialog({ caseId, onClose, onGewijzigd, onOpenTicket, ververs = 0, ingebed = false }) {
  const { zaak, setZaak, fout: laadFout, herlaad } = useCase(caseId, ververs)
  const [aanpassingen, setAanpassingen] = useState({})
  const [bezig, setBezig] = useState(false)
  const [melding, setMelding] = useState(null)


  // Wat in het formulier staat: aanpassingen van de analist, anders de waarde uit de case
  const waarden = Object.fromEntries(DOSSIERVELDEN.map((veld) => [veld, aanpassingen[veld] ?? zaak?.[veld] ?? '']))
  const wijzigingen = zaak
    ? Object.fromEntries(Object.entries(waarden).filter(([veld, waarde]) => waarde !== (zaak[veld] ?? '')))
    : {}
  const heeftWijzigingen = Object.keys(wijzigingen).length > 0

  function wijzig(veld, waarde) {
    setAanpassingen((huidig) => ({ ...huidig, [veld]: waarde }))
    setMelding(null)
  }

  async function opslaan(event) {
    event.preventDefault()
    if (!heeftWijzigingen) return
    if (waarden.status === 'Afgesloten' && !waarden.rootCause.trim()) {
      setMelding({ soort: 'fout', tekst: 'Vul de root cause in voordat je de case afsluit.' })
      return
    }

    setBezig(true)
    setMelding(null)
    try {
      const bijgewerkt = await updateCase(zaak.caseID, wijzigingen, zaak.versie)
      setZaak(bijgewerkt)
      setAanpassingen({})
      setMelding({ soort: 'succes', tekst: bijgewerkt.status === 'Afgesloten' ? 'Case afgesloten.' : 'Dossier opgeslagen.' })
      onGewijzigd()
    } catch (error) {
      setMelding({ soort: 'fout', tekst: error.message })
      // Een collega heeft de case gewijzigd: haal de nieuwste gegevens op
      if (error.status === 409) {
        setAanpassingen({})
        herlaad()
      }
    } finally {
      setBezig(false)
    }
  }

  const afgesloten = zaak?.status === 'Afgesloten'

  return (
    <Venster ingebed={ingebed} labelId="case-detail-titel" onClose={onClose}>
      <div className={styles.detailInhoud}>
        <header className={styles.detailHeader}>
          <div>
            <span className={styles.nummer}>{zaak?.caseNummer ?? 'Case'}</span>
            <h2 id="case-detail-titel">{zaak?.titel ?? 'Laden…'}</h2>
          </div>
          <button type="button" className={styles.sluit} onClick={onClose} aria-label="Details sluiten">×</button>
        </header>

        {laadFout && <p className={styles.fout} role="alert">{laadFout}</p>}

        {zaak && (
          <>
            <div className={styles.badges}>
              <span className={`${styles.status} ${styles[`status${zaak.status}`]}`}>{caseStatusLabel(zaak.status)}</span>
              <span className={`severity-badge ${PRIORITEIT_STIJL[zaak.prioriteit]}`}>
                <i className={`severity-dot ${PRIORITEIT_STIJL[zaak.prioriteit]}-dot`} />{zaak.prioriteit}
              </span>
            </div>

            <dl className={styles.gegevens}>
              <Gegeven label="Aangemaakt door">{zaak.aangemaaktDoorNaam}</Gegeven>
              <Gegeven label="Aangemaakt">{formatDatumTijd(zaak.aangemaaktOp)}</Gegeven>
              <Gegeven label="Afgesloten">{formatDatumTijd(zaak.afgeslotenOp)}</Gegeven>
              <Gegeven label="Tickets">{zaak.tickets.length}</Gegeven>
            </dl>

            <section className={styles.sectie}>
              <h3>Beschrijving</h3>
              <p className={styles.tekst}>{zaak.beschrijving}</p>
            </section>

            <section className={styles.sectie}>
              <h3>Gekoppelde tickets</h3>
              {zaak.tickets.length === 0 && <p className={styles.leeg}>Nog geen tickets gekoppeld.</p>}
              {zaak.tickets.length > 0 && (
                <ul className={styles.ticketLijst}>
                  {zaak.tickets.map((ticket) => (
                    <li key={ticket.ticketID}>
                      {/* Opent het ticketvenster boven op deze case */}
                      <button type="button" className={`${styles.nummer} ${styles.linkKnop}`} onClick={() => onOpenTicket(ticket.ticketID)}>
                        {ticket.ticketNummer}
                      </button>
                      <span className={styles.ticketLijstTitel}>{ticket.titel}</span>
                      <span className={`${styles.status} ${styles[`status${ticket.status}`]}`}>{statusLabel(ticket.status)}</span>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            {/* Wat de aanvaller deed en wanneer; los van de historie (wat analisten deden) */}
            <AanvalsTijdlijn
              zaak={zaak}
              afgesloten={afgesloten}
              onBijgewerkt={(bijgewerkt) => { setZaak(bijgewerkt); onGewijzigd() }}
            />

            {afgesloten ? (
              <section className={styles.sectie}>
                <h3>Onderzoeksdossier</h3>
                <h4 className={styles.subkop}>Root cause</h4>
                <p className={styles.tekst}>{zaak.rootCause ?? '—'}</p>
                <p className={styles.leeg}>Deze case is afgesloten en kan niet meer worden bewerkt.</p>
              </section>
            ) : (
              <form className={`${styles.sectie} ${styles.formulier}`} onSubmit={opslaan}>
                <h3>Onderzoeksdossier</h3>

                <label className={styles.veld}>
                  <span>Status</span>
                  <select value={waarden.status} onChange={(event) => wijzig('status', event.target.value)} disabled={bezig}>
                    {CASE_STATUSSEN.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
                  </select>
                </label>

                <label className={styles.veld}>
                  <span>Root cause</span>
                  <textarea
                    rows={4}
                    value={waarden.rootCause}
                    onChange={(event) => wijzig('rootCause', event.target.value)}
                    placeholder="Wat was de oorzaak van het incident?"
                    disabled={bezig}
                  />
                </label>

                <small className={styles.hint}>Een case afsluiten kan pas als de root cause is ingevuld.</small>

                <button type="submit" className={styles.knop} disabled={bezig || !heeftWijzigingen}>
                  {bezig ? 'Bezig…' : 'Dossier opslaan'}
                </button>
              </form>
            )}
          </>
        )}

        {melding && (
          <p className={melding.soort === 'fout' ? styles.fout : styles.succes} role={melding.soort === 'fout' ? 'alert' : 'status'}>
            {melding.tekst}
          </p>
        )}
      </div>
    </Venster>
  )
}

export default CaseDetailDialog
