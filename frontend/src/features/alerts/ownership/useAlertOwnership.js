import { useCallback } from 'react'

export default function useAlertOwnership(setAlerts) {
  const claimAlert = useCallback((alertId) => {
    setAlerts((currentAlerts) => currentAlerts.map((alert) =>
      alert.id === alertId && !alert.claimedByYou
        ? { ...alert, claimedByYou: true }
        : alert,
    ))
  }, [setAlerts])

  return { claimAlert }
}
