import { useCallback } from 'react'

import useAlerts from './useAlerts'
import AlertsContext from './AlertsContext'
import useAlertGenerator from './generator/useAlertGenerator'

function AlertsProvider({ query, children }) {
  const alertData = useAlerts(query)
  const generator = useAlertGenerator(alertData.testTemplates, alertData.addAlert)
  const { clearAlerts } = alertData
  const { pause } = generator
  const resetAlertList = useCallback(() => {
    pause()
    clearAlerts()
  }, [clearAlerts, pause])

  return (
    <AlertsContext.Provider value={{ ...alertData, generator, resetAlertList }}>
      {children}
    </AlertsContext.Provider>
  )
}

export default AlertsProvider
