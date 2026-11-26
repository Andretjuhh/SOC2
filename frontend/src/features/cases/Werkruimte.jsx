import { useEffect, useRef } from 'react'

import styles from '../tickets/TicketPage.module.css'

// Eén modaal venster waarin de case en een ticket naast elkaar staan.
// Elk paneel heeft een eigen sluitknop; Escape sluit eerst het ticket, daarna de case.
function Werkruimte({ aantal, onEscape, onSluitAlles, children }) {
  const dialogRef = useRef(null)

  useEffect(() => {
    const dialog = dialogRef.current
    if (dialog && !dialog.open) dialog.showModal()
    return () => dialog?.close()
  }, [])

  return (
    <dialog
      ref={dialogRef}
      className={`${styles.werkruimte}${aantal > 1 ? ` ${styles.werkruimteBreed}` : ''}`}
      aria-label="Case en ticket"
      onCancel={(event) => { event.preventDefault(); onEscape() }}
      // Klik naast de panelen (op de achtergrond) sluit alles
      onClick={(event) => {
        if (event.target === dialogRef.current || event.target.dataset.werkruimteRaster !== undefined) onSluitAlles()
      }}
    >
      <div className={styles.werkruimteRaster} data-werkruimte-raster="">
        {children}
      </div>
    </dialog>
  )
}

export default Werkruimte
