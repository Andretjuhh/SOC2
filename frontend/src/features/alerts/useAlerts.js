import { useCallback, useEffect, useMemo, useState } from 'react'

import { filterAlerts } from './filterAlerts'
import { getAlertMetrics } from './getAlertMetrics'

export default function useAlerts(query) {
  // useState keeps these values between renders; setters update the UI.
  const [severityFilter, setSeverityFilter] = useState('All events')
  const [alerts, setAlerts] = useState([])
  const [testTemplates, setTestTemplates] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    // Cancel the request if this hook's component unmounts.
    const controller = new AbortController()

    // Keep the effect synchronous; fetch runs in this async helper.
    async function loadAlerts() {
      try {
        // response is the server's HTTP result; data is its parsed JSON body.
        const response = await fetch('/api/alerts', { signal: controller.signal })
        if (!response.ok) {
          throw new Error(`Loading alerts failed (${response.status}).`)
        }

        const data = await response.json()
        // Adapt API field names and format timestamps for the UI.
        const mappedAlerts = data.map((alert) => ({
          id: alert.alertID,
          type: alert.title ?? alert.event ?? 'Untitled alert',
          severity: alert.severity,
          status: alert.status,
          owner: alert.owner ?? 'Unassigned',
          target: alert.affectedAsset,
          time: new Date(alert.detectionTime).toLocaleTimeString(),
          closedTime: alert.closedTime ? new Date(alert.closedTime).toLocaleString() : null,
        }))
        setAlerts(mappedAlerts)
        setTestTemplates(mappedAlerts)
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

  // Recalculate only when the alerts or filter inputs change.
  const filteredEvents = useMemo(
    () => filterAlerts(alerts, query, severityFilter),
    [alerts, query, severityFilter],
  )
  // Metrics depend on all alerts, not just the filtered list.
  const metrics = useMemo(() => getAlertMetrics(alerts), [alerts])
  const addAlert = useCallback((alert) => {
    setAlerts((currentAlerts) => [alert, ...currentAlerts])
  }, [])

  // Return the data and setters that the app's pages need.
  return {
    alerts,
    testTemplates,
    filteredEvents,
    loading,
    error,
    severityFilter,
    setSeverityFilter,
    metrics,
    addAlert,
  }
}