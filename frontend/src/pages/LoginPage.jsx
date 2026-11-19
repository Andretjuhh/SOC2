import { useState } from 'react'

function LoginPage() {
  const [gebruikersnaam, setGebruikersnaam] = useState('')
  const [wachtwoord, setWachtwoord] = useState('')
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')
  const [mfaChallenge, setMfaChallenge] = useState(null)

  async function handleLogin(event) {
    event.preventDefault()
    if (bezig) return

    setBezig(true)
    setFout('')
    setMfaChallenge(null)

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

      if (data?.mfaVereist && data.challengeId) {
        setMfaChallenge(data)
        setWachtwoord('')
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
    <main>
      <h1>Inloggen</h1>

      <form onSubmit={handleLogin} aria-busy={bezig}>
        <fieldset disabled={bezig}>
          <legend>Accountgegevens</legend>
          <p>
            <label htmlFor="gebruikersnaam">Gebruikersnaam</label>
            <br />
            <input
              id="gebruikersnaam"
              name="gebruikersnaam"
              autoComplete="username"
              value={gebruikersnaam}
              onChange={(event) => setGebruikersnaam(event.target.value)}
              required
            />
          </p>
          <p>
            <label htmlFor="wachtwoord">Wachtwoord</label>
            <br />
            <input
              id="wachtwoord"
              name="wachtwoord"
              type="password"
              autoComplete="current-password"
              value={wachtwoord}
              onChange={(event) => setWachtwoord(event.target.value)}
              required
            />
          </p>
          <button type="submit">{bezig ? 'Bezig met inloggen...' : 'Inloggen'}</button>
        </fieldset>
      </form>

      {fout && <p role="alert">{fout}</p>}
      {mfaChallenge && (
        <p role="status">Je inloggegevens zijn correct. Extra verificatie (MFA) is vereist; je bent nog niet ingelogd.</p>
      )}
    </main>
  )
}

export default LoginPage
