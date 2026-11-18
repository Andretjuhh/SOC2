import { useEffect, useState } from 'react'
import { Link, Navigate, Outlet } from 'react-router-dom'

function RequireLogin() {
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
  }, [])

  if (fout) return <main><p role="alert">{fout}</p><Link to="/login">Naar inloggen</Link></main>
  if (gebruiker === undefined) return <p role="status">Sessie controleren...</p>
  if (gebruiker === null) return <Navigate to="/login" replace />

  return <Outlet />
}

export default RequireLogin
