import { useEffect, useState } from 'react'

import { fetchTicket, rondTicketAf, updateTicket } from './api/ticketsApi'
import { STATUSSEN } from './ticketLabels'
import styles from './TicketPage.module.css'

// Afronden heeft een eigen formulier, dus die status staat niet in de keuzelijst
const BEWERKBARE_STATUSSEN = STATUSSEN.filter((status) => status.value !== 'Afgerond')

// Haalt de actuele versie van het ticket op. Die is nodig om te mogen bewerken of afronden.
function useTicketVersie(ticketId, laatstGewijzigdOp) {
  const [versie, setVersie] = useState(null)
  const [fout, setFout] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    fetchTicket(ticketId, controller.signal)
      .then((data) => { setVersie(data.versie); setFout('') })
      .catch((error) => { if (error.name !== 'AbortError') setFout(error.message) })

    return () => controller.abort()
  }, [ticketId, laatstGewijzigdOp])

  return { versie, setVersie, fout }
}

function TicketBewerken({ ticket, onGewijzigd }) {
  const { versie, setVersie, fout: versieFout } = useTicketVersie(ticket.ticketID, ticket.laatstGewijzigdOp)
  // Alleen wat de gebruiker heeft aangepast; de rest komt rechtstreeks uit het ticket
  const [aanpassingen, setAanpassingen] = useState({})
  const [oplossing, setOplossing] = useState('')
  const [bezig, setBezig] = useState(false)
  const [melding, setMelding] = useState(null)

  // Prioriteit en categorie liggen vast bij het aanmaken; status en beschrijving zijn te wijzigen
  const waarden = {
    status: aanpassingen.status ?? ticket.status,
    beschrijving: aanpassingen.beschrijving ?? ticket.beschrijving,
  }

  // Alleen de velden die echt anders zijn dan het ticket gaan mee naar de API
  const wijzigingen = Object.fromEntries(
    Object.entries(waarden).filter(([veld, waarde]) => waarde !== ticket[veld]),
  )
  const heeftWijzigingen = Object.keys(wijzigingen).length > 0

  function wijzig(veld, waarde) {
    setAanpassingen((huidig) => ({ ...huidig, [veld]: waarde }))
    setMelding(null)
  }

  async function voerUit(actie, succesTekst) {
    setBezig(true)
    setMelding(null)
    try {
      const bijgewerkt = await actie()
      setVersie(bijgewerkt.versie)
      setAanpassingen({})
      setMelding({ soort: 'succes', tekst: succesTekst })
      onGewijzigd()
      return true
    } catch (error) {
      setMelding({ soort: 'fout', tekst: error.message })
      // Bij een conflict heeft een collega het ticket gewijzigd: haal de nieuwste gegevens op
      if (error.status === 409) {
        setAanpassingen({})
        onGewijzigd()
      }
      return false
    } finally {
      setBezig(false)
    }
  }

  function opslaan(event) {
    event.preventDefault()
    if (!heeftWijzigingen) return
    if (!waarden.beschrijving.trim()) {
      setMelding({ soort: 'fout', tekst: 'De beschrijving mag niet leeg zijn.' })
      return
    }
    // Pas bij het opslaan: is de beschrijving van een open ticket gewijzigd en is de status
    // niet zelf aangepast, dan zetten we het ticket op In behandeling.
    const teVersturen = { ...wijzigingen }
    const automatisch = 'beschrijving' in teVersturen && !('status' in teVersturen) && ticket.status === 'Open'
    if (automatisch) teVersturen.status = 'InBehandeling'

    voerUit(
      () => updateTicket(ticket.ticketID, teVersturen, versie),
      automatisch ? 'Wijzigingen opgeslagen. Het ticket staat nu op In behandeling.' : 'Wijzigingen opgeslagen.',
    )
  }

  async function afronden(event) {
    event.preventDefault()
    if (!oplossing.trim()) {
      setMelding({ soort: 'fout', tekst: 'Het is verplicht om een oplossingsbeschrijving in te voeren voordat een ticket kan worden afgerond.' })
      return
    }
    const gelukt = await voerUit(() => rondTicketAf(ticket.ticketID, oplossing, versie), 'Ticket afgerond.')
    if (gelukt) setOplossing('')
  }

  if (ticket.status === 'Afgerond') {
    return (
      <section className={styles.sectie}>
        <h3>Bewerken</h3>
        <p className={styles.leeg}>Dit ticket is afgerond en kan niet meer worden bewerkt.</p>
        {melding?.soort === 'succes' && <p className={styles.succes} role="status">{melding.tekst}</p>}
      </section>
    )
  }

  const nietKlaar = bezig || !versie

  return (
    <>
      <form className={`${styles.sectie} ${styles.formulier}`} onSubmit={opslaan}>
        <h3>Bewerken</h3>

        <label className={styles.veld}>
          <span>Status</span>
          <select value={waarden.status} onChange={(event) => wijzig('status', event.target.value)} disabled={bezig}>
            {BEWERKBARE_STATUSSEN.map((optie) => <option key={optie.value} value={optie.value}>{optie.label}</option>)}
          </select>
        </label>

        <label className={styles.veld}>
          <span>Beschrijving</span>
          <textarea
            rows={7}
            value={waarden.beschrijving}
            onChange={(event) => wijzig('beschrijving', event.target.value)}
            disabled={bezig}
            aria-describedby={ticket.status === 'Open' ? 'beschrijving-hint' : undefined}
          />
        </label>
        {ticket.status === 'Open' && (
          <small id="beschrijving-hint" className={styles.hint}>
            Sla je een gewijzigde beschrijving op, dan gaat de status automatisch naar In behandeling.
          </small>
        )}

        <button type="submit" className={styles.knop} disabled={nietKlaar || !heeftWijzigingen}>
          {bezig ? 'Bezig…' : 'Wijzigingen opslaan'}
        </button>
      </form>

      <form className={`${styles.sectie} ${styles.formulier}`} onSubmit={afronden}>
        <h3>Afronden</h3>
        <label className={styles.veld}>
          <span>Oplossingsbeschrijving (verplicht)</span>
          <textarea
            rows={4}
            value={oplossing}
            onChange={(event) => { setOplossing(event.target.value); setMelding(null) }}
            placeholder="Beschrijf wat er is gedaan en wat de oorzaak was."
            disabled={bezig}
          />
        </label>
        <button type="submit" className={`${styles.knop} ${styles.knopAfronden}`} disabled={nietKlaar}>
          {bezig ? 'Bezig…' : 'Ticket afronden'}
        </button>
      </form>

      {versieFout && <p className={styles.fout} role="alert">{versieFout}</p>}
      {melding && (
        <p className={melding.soort === 'fout' ? styles.fout : styles.succes} role={melding.soort === 'fout' ? 'alert' : 'status'}>
          {melding.tekst}
        </p>
      )}
    </>
  )
}

export default TicketBewerken
