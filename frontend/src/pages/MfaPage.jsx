import { useEffect, useRef, useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import Icon from '../layouts/Icon.jsx'
import '../styles/Mfa.css'

const LENGTE = 6
const FOUT_CODE = 'De ingevoerde verificatiecode is onjuist of verlopen. Probeer het opnieuw.'
const FOUT_SESSIE = 'Je verificatiesessie is verlopen. Log opnieuw in.'

function resterendeSeconden(verlooptOp) {
  return Math.max(0, Math.floor((new Date(verlooptOp).getTime() - Date.now()) / 1000))
}

function formatTijd(seconden) {
  const m = String(Math.floor(seconden / 60)).padStart(2, '0')
  const s = String(seconden % 60).padStart(2, '0')
  return `${m}:${s}`
}

function MfaPage() {
  const { state } = useLocation()
  const navigate = useNavigate()
  const challengeId = state?.challengeId
  const verlooptOp = state?.verlooptOp

  const [cijfers, setCijfers] = useState(() => Array(LENGTE).fill(''))
  const [bezig, setBezig] = useState(false)
  const [fout, setFout] = useState('')
  const [resterend, setResterend] = useState(() => resterendeSeconden(verlooptOp))
  const velden = useRef([])

  useEffect(() => {
    velden.current[0]?.focus()
  }, [])

  useEffect(() => {
    if (!challengeId) return undefined
    const timer = setInterval(() => setResterend(resterendeSeconden(verlooptOp)), 1000)
    return () => clearInterval(timer)
  }, [challengeId, verlooptOp])

  useEffect(() => {
    if (challengeId && resterend === 0) {
      navigate('/login', { replace: true, state: { melding: FOUT_SESSIE } })
    }
  }, [challengeId, resterend, navigate])

  if (!challengeId) return <Navigate to="/login" replace />

  function leegmaken() {
    setCijfers(Array(LENGTE).fill(''))
    velden.current[0]?.focus()
  }

  async function verstuur(code) {
    if (bezig) return
    setBezig(true)
    setFout('')

    try {
      const response = await fetch('/api/auth/mfa/verify', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ challengeId, code }),
      })

      if (response.ok) {
        navigate('/overview', { replace: true })
        return
      }

      const data = await response.json().catch(() => null)
      if (response.status === 401 && data?.challengeVerlopen) {
        navigate('/login', { replace: true, state: { melding: FOUT_SESSIE } })
        return
      }
      if (response.status === 401) setFout(FOUT_CODE)
      else if (response.status === 429) setFout('Te veel pogingen. Probeer het over een minuut opnieuw.')
      else setFout('Verifiëren is niet gelukt. Probeer het opnieuw.')
      leegmaken()
    } catch {
      setFout('De server is niet bereikbaar. Probeer het later opnieuw.')
      leegmaken()
    } finally {
      setBezig(false)
    }
  }

  function vul(start, tekst) {
    const nieuw = tekst.replace(/\D/g, '')
    if (!nieuw) return
    const volgende = [...cijfers]
    let i = start
    for (const c of nieuw) {
      if (i >= LENGTE) break
      volgende[i++] = c
    }
    setCijfers(volgende)
    setFout('')
    if (volgende.every(Boolean)) verstuur(volgende.join(''))
    else velden.current[Math.min(i, LENGTE - 1)]?.focus()
  }

  function wis(index) {
    setCijfers((huidig) => huidig.map((c, i) => (i === index ? '' : c)))
  }

  function handleChange(index, waarde) {
    if (waarde === '') wis(index)
    else vul(waarde.length > 1 ? 0 : index, waarde)
  }

  function handleKeyDown(index, event) {
    if (event.key === 'Backspace' && !cijfers[index] && index > 0) {
      event.preventDefault()
      wis(index - 1)
      velden.current[index - 1]?.focus()
    } else if (event.key === 'ArrowLeft' && index > 0) {
      velden.current[index - 1]?.focus()
    } else if (event.key === 'ArrowRight' && index < LENGTE - 1) {
      velden.current[index + 1]?.focus()
    }
  }

  function handlePaste(index, event) {
    event.preventDefault()
    vul(index, event.clipboardData.getData('text'))
  }

  function handleSubmit(event) {
    event.preventDefault()
    if (cijfers.every(Boolean)) verstuur(cijfers.join(''))
  }

  async function annuleer() {
    try {
      await fetch('/api/auth/mfa/cancel', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ challengeId }),
      })
    } catch {
      // De challenge verloopt vanzelf; de gebruiker gaat in elk geval terug naar het inlogscherm.
    }
    navigate('/login', { replace: true })
  }

  return (
    <main className="mfa-page">
      <section className="mfa-card" aria-labelledby="mfa-titel">
        <span className="mfa-brand">application name</span>
        <span className="mfa-icon"><Icon name="shield" size={22} /></span>
        <h1 id="mfa-titel">Verificatie in twee stappen</h1>
        <p className="mfa-uitleg">
          Open Microsoft Authenticator en voer de actuele 6-cijferige code van dit account in om in te loggen.
        </p>

        <form onSubmit={handleSubmit} aria-busy={bezig}>
          <div className="mfa-velden" role="group" aria-label="Verificatiecode van 6 cijfers">
            {cijfers.map((cijfer, index) => (
              <input
                key={index}
                ref={(el) => { velden.current[index] = el }}
                className={`mfa-veld${fout ? ' mfa-veld-fout' : ''}`}
                aria-label={`Cijfer ${index + 1} van ${LENGTE}`}
                aria-invalid={Boolean(fout)}
                inputMode="numeric"
                autoComplete={index === 0 ? 'one-time-code' : 'off'}
                value={cijfer}
                readOnly={bezig}
                onFocus={(event) => event.target.select()}
                onChange={(event) => handleChange(index, event.target.value)}
                onKeyDown={(event) => handleKeyDown(index, event)}
                onPaste={(event) => handlePaste(index, event)}
              />
            ))}
          </div>

          <p className="mfa-fout" role="alert">{fout}</p>

          <button className="mfa-knop" type="submit" disabled={bezig || !cijfers.every(Boolean)}>
            {bezig ? 'Bezig met verifiëren...' : 'Verifiëren'}
          </button>
        </form>

        <p className="mfa-timer"><Icon name="clock" size={13} /> Sessie verloopt over {formatTijd(resterend)}</p>
        <button className="mfa-terug" type="button" onClick={annuleer}>Terug naar inloggen</button>
      </section>
    </main>
  )
}

export default MfaPage
