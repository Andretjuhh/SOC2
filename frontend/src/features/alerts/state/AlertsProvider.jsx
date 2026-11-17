import { useCallback } from 'react'

import AlertsContext from './AlertsContext'
import useAlerts from './useAlerts'
import useAlertGenerator from '../generator/useAlertGenerator'

function AlertsProvider({ query, children }) {
  const alertData = useAlerts(query)
  const generator = useAlertGenerator(alertData.testTemplates, alertData.addAlert)
  const { refreshAlerts } = alertData
  const { pause } = generator
  const resetAlertList = useCallback(() => {
    pause()
    refreshAlerts()
  }, [pause, refreshAlerts])

  return (
    <AlertsContext.Provider value={{ ...alertData, generator, resetAlertList }}>
      {children}
    </AlertsContext.Provider>
  )
}

export default AlertsProvider
