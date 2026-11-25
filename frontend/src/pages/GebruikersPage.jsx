import { useContext, useEffect, useState } from 'react'
import SessionContext from '../features/auth/SessionContext'

async function lees(url) {
  const response = await fetch(url, { credentials: 'include' })
  if (!response.ok) throw new Error('Gebruikersbeheer kon niet worden geladen.')
  return response.json()
}

function GebruikerRij({ gebruiker, rollen, klanten, onOpgeslagen }) {
  const [rolId, setRolId] = useState(gebruiker.rolId)
  const [klantId, setKlantId] = useState(gebruiker.klantId || '')
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')

  async function opslaan() {
    setBezig(true)
    setFout('')
    try {
      const response = await fetch(`/api/gebruikers/${gebruiker.id}/rol`, {
        method: 'PUT',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ rolId, klantId: klantId || null }),
      })
      if (!response.ok) {
        const data = await response.json().catch(() => null)
        throw new Error(data?.melding || 'Rol toekennen is niet gelukt.')
      }
      onOpgeslagen(gebruiker.gebruikersnaam)
    } catch (error) {
      setFout(error.message)
    } finally {
      setBezig(false)
    }
  }

  return (
    <tr>
      <td>{gebruiker.gebruikersnaam}</td>
      <td>{gebruiker.naam}</td>
      <td>
        <select aria-label={`Rol voor ${gebruiker.gebruikersnaam}`} value={rolId} onChange={(event) => setRolId(event.target.value)} disabled={bezig}>
          {!rollen.some((rol) => rol.id === gebruiker.rolId) && <option value={gebruiker.rolId} disabled>{gebruiker.rol}</option>}
          {rollen.map((rol) => <option key={rol.id} value={rol.id}>{rol.naam}</option>)}
        </select>
      </td>
      <td>
        <select aria-label={`Klant voor ${gebruiker.gebruikersnaam}`} value={klantId} onChange={(event) => setKlantId(event.target.value)} disabled={bezig}>
          <option value="">Geen klant</option>
          {klanten.map((klant) => <option key={klant.id} value={klant.id}>{klant.naam}</option>)}
        </select>
      </td>
      <td>
        <button type="button" onClick={opslaan} disabled={bezig}>{bezig ? 'Opslaan...' : 'Opslaan'}</button>
        {fout && <p role="alert">{fout}</p>}
      </td>
    </tr>
  )
}

function GebruikersPage() {
  const sessie = useContext(SessionContext)
  const [gegevens, setGegevens] = useState(null)
  const [fout, setFout] = useState('')
  const [melding, setMelding] = useState('')

  useEffect(() => {
    if (!sessie?.magGebruikersBeheren) return
    let actief = true
    Promise.all([lees('/api/gebruikers'), lees('/api/rollen'), lees('/api/klanten')])
      .then(([gebruikers, rollen, klanten]) => { if (actief) setGegevens({ gebruikers, rollen, klanten }) })
      .catch((error) => { if (actief) setFout(error.message) })
    return () => { actief = false }
  }, [sessie?.magGebruikersBeheren])

  if (!sessie?.magGebruikersBeheren) return <p role="alert">Je hebt geen toegang tot gebruikersbeheer.</p>
  if (fout) return <p role="alert">{fout}</p>
  if (!gegevens) return <p role="status">Gebruikers laden...</p>

  return (
    <section className="panel">
      <div className="panel-heading">
        <div><h1>Gebruikers en rollen</h1><p>Nieuwe accounts beginnen zonder rechten. Ken hier een rol en klant toe.</p></div>
      </div>
      {melding && <p role="status">{melding}</p>}
      <div className="table-scroll">
        <table className="alerts-table">
          <thead><tr><th>Account</th><th>Naam</th><th>Rol</th><th>Klant</th><th>Actie</th></tr></thead>
          <tbody>
            {gegevens.gebruikers.map((gebruiker) => (
              <GebruikerRij key={gebruiker.id} gebruiker={gebruiker} rollen={gegevens.rollen} klanten={gegevens.klanten}
                onOpgeslagen={(naam) => setMelding(`Toegang voor ${naam} opgeslagen. De gebruiker kan de toegang opnieuw controleren.`)} />
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}

export default GebruikersPage
