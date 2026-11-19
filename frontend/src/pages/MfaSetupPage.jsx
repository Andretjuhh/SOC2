import { useState } from 'react'
import { Link } from 'react-router-dom'
import QRCode from 'qrcode'
import Icon from '../layouts/Icon.jsx'
import '../styles/Mfa.css'

// Alleen voor lokale ontwikkeling: koppelt Microsoft Authenticator aan een bestaand testaccount.
function MfaSetupPage() {
  const [gebruikersnaam, setGebruikersnaam] = useState('')
  const [wachtwoord, setWachtwoord] = useState('')
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')
  const [resultaat, setResultaat] = useState(null)

  async function handleSubmit(event) {
    event.preventDefault()
    if (bezig) return
    setBezig(true)
    setFout('')

    try {
      const response = await fetch('/api/gebruikers/mfa-setup', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ gebruikersnaam: gebruikersnaam.trim(), wachtwoord }),
      })
      if (response.status === 401) {
        setFout('Ongeldige inloggegevens.')
        return
      }
      if (!response.ok) {
        setFout('Koppelen is niet gelukt. Probeer het opnieuw.')
        return
      }
      const data = await response.json()
      const qr = await QRCode.toDataURL(data.otpAuthUri, { width: 220, margin: 2 })
      setResultaat({ qr, sleutel: data.sleutel })
      setWachtwoord('')
    } catch {
      setFout('De server is niet bereikbaar. Probeer het later opnieuw.')
    } finally {
      setBezig(false)
    }
  }

  return (
    <main className="mfa-page">
      <section className="mfa-card" aria-labelledby="setup-titel">
        <span className="mfa-brand">application name</span>
        <span className="mfa-icon"><Icon name="shield" size={22} /></span>
        <h1 id="setup-titel">Microsoft Authenticator koppelen</h1>

        {!resultaat ? (
          <>
            <p className="mfa-uitleg">Bevestig je account om een QR-code te genereren. Let op: een bestaande koppeling wordt vervangen.</p>
            <form onSubmit={handleSubmit} aria-busy={bezig}>
              <label className="mfa-label" htmlFor="setup-gebruikersnaam">Gebruikersnaam</label>
              <input id="setup-gebruikersnaam" className="mfa-input" autoComplete="username"
                value={gebruikersnaam} onChange={(e) => setGebruikersnaam(e.target.value)} required />
              <label className="mfa-label" htmlFor="setup-wachtwoord">Wachtwoord</label>
              <input id="setup-wachtwoord" className="mfa-input" type="password" autoComplete="current-password"
                value={wachtwoord} onChange={(e) => setWachtwoord(e.target.value)} required />
              <p className="mfa-fout" role="alert">{fout}</p>
              <button className="mfa-knop" type="submit" disabled={bezig}>
                {bezig ? 'Bezig...' : 'QR-code genereren'}
              </button>
            </form>
          </>
        ) : (
          <>
            <ol className="mfa-stappen">
              <li>Open <strong>Microsoft Authenticator</strong> en tik op <strong>+</strong>.</li>
              <li>Kies <strong>Ander account (Google, Facebook, enzovoort)</strong>.</li>
              <li>Scan de QR-code hieronder.</li>
            </ol>
            <img className="mfa-qr" src={resultaat.qr} alt="QR-code om het account in Microsoft Authenticator toe te voegen" width="220" height="220" />
            <p className="mfa-uitleg">Scannen lukt niet? Kies in de app <em>Of voer code handmatig in</em> en gebruik deze sleutel:</p>
            <code className="mfa-sleutel">{resultaat.sleutel}</code>
            <Link className="mfa-knop mfa-link" to="/login">Naar inloggen</Link>
          </>
        )}
      </section>
    </main>
  )
}

export default MfaSetupPage
