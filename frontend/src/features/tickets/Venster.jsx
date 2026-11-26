import { useEffect, useRef, useState } from 'react'

import styles from './TicketPage.module.css'

// Elementen in de kopregel die gewoon klikbaar moeten blijven (sluitknop, links, ...)
const GEEN_SLEEPGREEP = 'button, a, input, select, textarea'

// Hoogste laag tot nu toe. Het venster waar je het laatst op klikt, krijgt de hoogste en komt dus vooraan.
let hoogsteLaag = 1

// Laat een ingebed paneel naar voren komen zodra je erop klikt of erin tabt
function useVooraan() {
  const [laag, setLaag] = useState(() => ++hoogsteLaag)   // een nieuw venster opent vooraan

  function naarVoren() {
    if (laag !== hoogsteLaag) setLaag(++hoogsteLaag)
  }

  return { laag, naarVoren }
}

// Maakt een venster versleepbaar aan de kopregel. Dubbelklik op de kopregel zet het terug.
function useSleepbaar() {
  const [positie, setPositie] = useState({ x: 0, y: 0 })
  const start = useRef(null)

  function opKop(event) {
    return event.target.closest(`.${styles.detailHeader}`) && !event.target.closest(GEEN_SLEEPGREEP)
  }

  return {
    stijl: positie.x || positie.y ? { transform: `translate(${positie.x}px, ${positie.y}px)` } : undefined,
    handlers: {
      onPointerDown(event) {
        if (event.button !== 0 || !opKop(event)) return
        start.current = { muisX: event.clientX, muisY: event.clientY, x: positie.x, y: positie.y }
        event.currentTarget.setPointerCapture(event.pointerId)
      },
      onPointerMove(event) {
        if (!start.current) return
        // Niet verder dan de rand van het scherm, zodat de kopregel altijd te pakken blijft
        const maxX = window.innerWidth / 2
        const maxY = window.innerHeight / 2
        setPositie({
          x: Math.max(-maxX, Math.min(maxX, start.current.x + event.clientX - start.current.muisX)),
          y: Math.max(-maxY, Math.min(maxY, start.current.y + event.clientY - start.current.muisY)),
        })
      },
      onPointerUp(event) {
        if (!start.current) return
        start.current = null
        event.currentTarget.releasePointerCapture?.(event.pointerId)
      },
      onDoubleClick(event) {
        if (opKop(event)) setPositie({ x: 0, y: 0 })
      },
    },
  }
}

// Omhulsel voor het ticket- en casevenster.
// Los: een native <dialog> (focusbeheer, Escape en een backdrop).
// Ingebed: een paneel binnen de werkruimte, zodat case en ticket naast elkaar open kunnen staan.
// In beide gevallen te verslepen aan de kopregel.
function Venster({ ingebed = false, labelId, onClose, children }) {
  const dialogRef = useRef(null)
  const { stijl, handlers } = useSleepbaar()
  const { laag, naarVoren } = useVooraan()

  useEffect(() => {
    if (ingebed) return undefined
    const dialog = dialogRef.current
    if (dialog && !dialog.open) dialog.showModal()
    return () => dialog?.close()
  }, [ingebed])

  if (ingebed) {
    return (
      <section
        className={`${styles.paneel} ${styles.sleepbaar}`}
        aria-labelledby={labelId}
        style={{ ...stijl, zIndex: laag }}
        {...handlers}
        onPointerDown={(event) => { naarVoren(); handlers.onPointerDown(event) }}
        onFocus={naarVoren}
      >
        {children}
      </section>
    )
  }

  return (
    <dialog
      ref={dialogRef}
      className={`${styles.detail} ${styles.sleepbaar}`}
      aria-labelledby={labelId}
      style={stijl}
      {...handlers}
      onCancel={(event) => { event.preventDefault(); onClose() }}
      onClick={(event) => { if (event.target === dialogRef.current) onClose() }}
    >
      {children}
    </dialog>
  )
}

export default Venster
