import { useCallback, useEffect, useMemo, useState } from 'react'

import { filterAlerts } from './filterAlerts'
import { getAlertMetrics } from './getAlertMetrics'
import useAlertOwnership from './ownership/useAlertOwnership'

async function fetchAlerts(signal) {
  const requestConfig = { signal }
  const response = await fetch('/api/alerts', requestConfig)

  const requestWasSuccessful = response.ok

  if (requestWasSuccessful === false) {
    const statusCode = response.status
    const errorMessage = `Loading alerts failed (${statusCode}).`
    throw new Error(errorMessage)
  }

  const data = await response.json()

  return data
}

function mapAlertData(data) {
  const mappedAlerts = data.map((alert) => {
    const hasTitle = alert.title !== null && alert.title !== undefined
    const hasEvent = alert.event !== null && alert.event !== undefined
    const hasOwner = alert.owner !== null && alert.owner !== undefined
    const hasClosedTime = alert.closedTime !== null && alert.closedTime !== undefined

    let alertType = 'Untitled alert'

    if (hasTitle === true) {
      alertType = alert.title
    } else if (hasEvent === true) {
      alertType = alert.event
    }

    let ownerName = 'Unassigned'

    if (hasOwner === true) {
      ownerName = alert.owner
    }

    let closedTimeValue = null

    if (hasClosedTime === true) {
      closedTimeValue = new Date(alert.closedTime).toLocaleString()
    }

    return {
      id: alert.alertID,
      type: alertType,
      severity: alert.severity,
      status: alert.status,
      owner: ownerName,
      target: alert.affectedAsset,
      time: new Date(alert.detectionTime).toLocaleTimeString(),
      closedTime: closedTimeValue,
    }
  })

  return mappedAlerts
}

export default function useAlerts(query) {
  const [severityFilter, setSeverityFilter] = useState('All events')
  const [alerts, setAlerts] = useState([])
  const [testTemplates, setTestTemplates] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadAlerts() {
      try {
        const data = await fetchAlerts(controller.signal)
        const mappedAlerts = mapAlertData(data)

        setTestTemplates(mappedAlerts)
      } catch (requestError) {
        const isNotAbortError = requestError.name !== 'AbortError'

        if (isNotAbortError === true) {
          setError(requestError.message)
        }
      } finally {
        const shouldStopLoading = controller.signal.aborted === false

        if (shouldStopLoading === true) {
          setLoading(false)
        }
      }
    }

    loadAlerts()

    return () => {
      controller.abort()
    }
  }, [])

  const filteredEvents = useMemo(
    () => filterAlerts(alerts, query, severityFilter),
    [alerts, query, severityFilter],
  )

  const metrics = useMemo(() => getAlertMetrics(alerts), [alerts])

  const addAlert = useCallback((alert) => {
    setAlerts((currentAlerts) => {
      const updatedAlerts = [alert, ...currentAlerts]
      return updatedAlerts
    })
  }, [])

  const { claimAlert } = useAlertOwnership(setAlerts)

  const clearAlerts = useCallback(() => {
    setAlerts([])
  }, [])

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
    claimAlert,
    clearAlerts,
  }
}