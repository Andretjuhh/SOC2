import { useContext } from 'react'

import AlertsContext from './AlertsContext'

function useAlertData() {
  const alertData = useContext(AlertsContext)

  if (alertData === null) {
    throw new Error('useAlertData must be used within an AlertsProvider.')
  }

  return alertData
}

export default useAlertData
