import { useCallback } from 'react'

export default function useAlertOwnership(setAlerts) {

  // Markeert een alert als geclaimd en onthoudt het ticket dat ervoor is aangemaakt.
  const claimAlert = useCallback((alertId, ticket = null) => {

    setAlerts((currentAlerts) => {

      return currentAlerts.map((alert) => {

        const isMatchingAlert = alert.id === alertId
        const isNotAlreadyClaimed = !alert.claimedByYou

        if (isMatchingAlert && isNotAlreadyClaimed) {
          return {
            ...alert,
            claimedByYou: true,
            ticketId: ticket?.ticketID ?? null,
            ticketNummer: ticket?.ticketNummer ?? null,
          }
        }

        return alert
      })
    })
  }, [setAlerts])

  return { claimAlert }
}

