import { useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import Icon from '../layouts/Icon.jsx'
import '../styles/Mfa.css'

function LoginPage() {
  const navigate = useNavigate()
  const { state } = useLocation()
  const [gebruikersnaam, setGebruikersnaam] = useState('')
  const [wachtwoord, setWachtwoord] = useState('')
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')

  async function handleLogin(event) {
    event.preventDefault()
    if (bezig) return

    setBezig(true)
    setFout('')

    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ gebruikersnaam: gebruikersnaam.trim(), wachtwoord }),
      })
      const data = await response.json().catch(() => null)

      if (!response.ok) {
        if (response.status === 401) {
          setFout('Ongeldige inloggegevens.')
        } else if (response.status === 429) {
          setFout('Te veel inlogpogingen. Probeer het over een minuut opnieuw.')
        } else if (response.status === 400 && data?.errors) {
          setFout(Object.values(data.errors).flat().join(' '))
        } else {
          setFout('Inloggen is niet gelukt. Probeer het opnieuw.')
        }
        return
      }

      if (data?.mfaVereist === false) {
        navigate('/overview', { replace: true })
      } else if (data?.mfaVereist && data.challengeId) {
        // challengeId alleen in router-state (niet in de URL of opslag).
        navigate('/mfa', {
          replace: true,
          state: { challengeId: data.challengeId, verlooptOp: data.verlooptOp },
        })
      } else {
        setFout('De server gaf geen geldige inlogreactie terug.')
      }
    } catch {
      setFout('De server is niet bereikbaar. Probeer het later opnieuw.')
    } finally {
      setBezig(false)
    }
  }

  return (
    <main className="mfa-page">
      <section className="mfa-card" aria-labelledby="login-titel">
        <span className="mfa-brand">SOC2</span>
        <span className="mfa-icon"><Icon name="shield" size={22} /></span>
        <h1 id="login-titel">Inloggen</h1>
        <p className="mfa-uitleg">Log in met je account om verder te gaan.</p>

        {state?.melding && <p className="mfa-uitleg" role="status">{state.melding}</p>}

        <form onSubmit={handleLogin} aria-busy={bezig}>
          <fieldset className="mfa-accountvelden" aria-label="Accountgegevens" disabled={bezig}>
            <label className="mfa-label" htmlFor="gebruikersnaam">Gebruikersnaam</label>
            <input
              className="mfa-input"
              id="gebruikersnaam"
              name="gebruikersnaam"
              autoComplete="username"
              value={gebruikersnaam}
              onChange={(event) => setGebruikersnaam(event.target.value)}
              required
            />
            <label className="mfa-label" htmlFor="wachtwoord">Wachtwoord</label>
            <input
              className="mfa-input"
              id="wachtwoord"
              name="wachtwoord"
              type="password"
              autoComplete="current-password"
              value={wachtwoord}
              onChange={(event) => setWachtwoord(event.target.value)}
              required
            />
            <p className="mfa-fout" role="alert">{fout}</p>
            <button className="mfa-knop" type="submit">{bezig ? 'Bezig met inloggen...' : 'Inloggen'}</button>
          </fieldset>
        </form>
      </section>
    </main>
  )
}

export default LoginPage
