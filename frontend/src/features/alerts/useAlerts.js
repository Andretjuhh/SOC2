import { useEffect, useMemo, useState } from 'react'

import { filterAlerts } from './filterAlerts'
import { getAlertMetrics } from './getAlertMetrics'

export default function useAlerts(query) {
  const [severityFilter, setSeverityFilter] = useState('All events')
  const [alerts, setAlerts] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadAlerts() {
      try {
        const response = await fetch('/api/alerts', { signal: controller.signal })
        if (!response.ok) {
          throw new Error(`Loading alerts failed (${response.status}).`)
        }

        const data = await response.json()
        setAlerts(data.map((alert) => ({
          id: alert.alertID,
          type: alert.event,
          severity: alert.severity,
          target: alert.affectedAsset,
          time: new Date(alert.detectionTime).toLocaleTimeString(),
          closedTime: alert.closedTime ? new Date(alert.closedTime).toLocaleString() : null,
        })))
      } catch (requestError) {
        if (requestError.name !== 'AbortError') {
          setError(requestError.message)
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    loadAlerts()
    return () => controller.abort()
  }, [])

  const filteredEvents = useMemo(
    () => filterAlerts(alerts, query, severityFilter),
    [alerts, query, severityFilter],
  )
  const metrics = useMemo(() => getAlertMetrics(alerts), [alerts])

  return {
    alerts,
    filteredEvents,
    loading,
    error,
    severityFilter,
    setSeverityFilter,
    metrics,
  }
}