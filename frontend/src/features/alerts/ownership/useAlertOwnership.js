import { useCallback } from 'react'

export default function useAlertOwnership(setAlerts) {

  const claimAlert = useCallback((alertId) => {

    setAlerts((currentAlerts) => {

      return currentAlerts.map((alert) => {

        const isMatchingAlert = alert.id === alertId
        const isNotAlreadyClaimed = !alert.claimedByYou

        if (isMatchingAlert && isNotAlreadyClaimed) {
          return {  ...alert, claimedByYou: true, }
        }

        return alert
      })
    })
  }, [setAlerts])

  return { claimAlert }
}

