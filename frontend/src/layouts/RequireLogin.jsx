import { useEffect, useState } from 'react'
import { Link, Navigate, Outlet, useLocation } from 'react-router-dom'
import SessionContext from '../features/auth/SessionContext'

function RequireLogin() {
  const location = useLocation()
  const [gebruiker, setGebruiker] = useState(undefined)
  const [fout, setFout] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function controleerSessie() {
      try {
        const response = await fetch('/api/auth/me', {
          credentials: 'include',
          signal: controller.signal,
        })
        if (response.status === 401) {
          setGebruiker(null)
          return
        }
        if (!response.ok) throw new Error('Sessie controleren is niet gelukt.')
        setGebruiker(await response.json())
      } catch (error) {
        if (error.name !== 'AbortError') setFout('Je sessie kon niet worden gecontroleerd. Probeer opnieuw in te loggen.')
      }
    }

    controleerSessie()
    return () => controller.abort()
  }, [location.pathname])

  if (fout) return <main><p role="alert">{fout}</p><Link to="/login">Naar inloggen</Link></main>
  if (gebruiker === undefined) return <p role="status">Sessie controleren...</p>
  if (gebruiker === null) return <Navigate to="/login" replace />

  if (!gebruiker.magDashboard) {
    return (
      <main className="mfa-page">
        <section className="mfa-card">
          <h1>Wachten op toegang</h1>
          <p className="mfa-uitleg">{gebruiker.melding || 'Je hebt nog geen toegang tot het dashboard.'}</p>
          <button className="mfa-knop" type="button" onClick={() => window.location.reload()}>Opnieuw controleren</button>
          <Link className="mfa-terug" to="/login">Terug naar inloggen</Link>
        </section>
      </main>
    )
  }

  return <SessionContext.Provider value={gebruiker}><Outlet /></SessionContext.Provider>
}

export default RequireLogin
