import { useCallback, useEffect, useMemo, useState } from 'react'

import { filterAlerts } from '../utils/filterAlerts'
import { getAlertMetrics } from '../utils/getAlertMetrics'

const ALERT_POLL_INTERVAL_MS = 10_000

async function fetchAlerts(url, signal) {
  const requestConfig = { signal }
  const response = await fetch(url, requestConfig)

  const requestWasSuccessful = response.ok

  if (requestWasSuccessful === false) {
    const statusCode = response.status
    const errorMessage = `Request to ${url} failed (${statusCode}).`
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
      id: alert.alertId ?? alert.alertID,
      type: alertType,
      severity: alert.severity,
      status: alert.status ?? (hasClosedTime ? 'Closed' : 'New'),
      owner: ownerName,
      isClaimed: hasOwner,
      claimedByYou: alert.claimedByYou === true,
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
  const [claimNotice, setClaimNotice] = useState('')
  const [claimingAlertId, setClaimingAlertId] = useState(null)

  const refreshAlerts = useCallback(async (signal) => {
    try {
      const data = await fetchAlerts('/api/alerts', signal)
      const mappedAlerts = mapAlertData(data)

      setAlerts(mappedAlerts)
      setError('')
      return true
    } catch (requestError) {
      const isNotAbortError = requestError.name !== 'AbortError'

      if (isNotAbortError === true) {
        setError(requestError.message)
      }

      return false
    }
  }, [])

  useEffect(() => {
    const controller = new AbortController()

    async function loadAlerts() {
      try {
        const [templates] = await Promise.all([
          fetchAlerts('/api/alerts/templates', controller.signal),
          refreshAlerts(controller.signal),
        ])

        setTestTemplates(mapAlertData(templates))
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
  }, [refreshAlerts])

  useEffect(() => {
    if (loading) {
      return undefined
    }

    let timeoutId
    let requestController
    let requestInProgress = false
    let disposed = false

    const scheduleNextPoll = () => {
      if (!disposed && document.visibilityState === 'visible') {
        timeoutId = window.setTimeout(refreshOnTimer, ALERT_POLL_INTERVAL_MS)
      }
    }

    const refreshOnTimer = async () => {
      if (disposed || document.visibilityState !== 'visible' || requestInProgress) {
        return
      }

      requestInProgress = true
      requestController = new AbortController()
      try {
        await refreshAlerts(requestController.signal)
      } finally {
        requestInProgress = false
        requestController = undefined
        scheduleNextPoll()
      }
    }

    const refreshImmediately = () => {
      window.clearTimeout(timeoutId)
      timeoutId = undefined
      if (!requestInProgress) {
        refreshOnTimer()
      }
    }

    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') {
        refreshImmediately()
      } else {
        window.clearTimeout(timeoutId)
        timeoutId = undefined
      }
    }

    scheduleNextPoll()
    window.addEventListener('online', refreshImmediately)
    document.addEventListener('visibilitychange', handleVisibilityChange)

    return () => {
      disposed = true
      window.clearTimeout(timeoutId)
      requestController?.abort()
      window.removeEventListener('online', refreshImmediately)
      document.removeEventListener('visibilitychange', handleVisibilityChange)
    }
  }, [loading, refreshAlerts])

  const filteredEvents = useMemo(
    () => filterAlerts(alerts, query, severityFilter),
    [alerts, query, severityFilter],
  )

  const metrics = useMemo(() => getAlertMetrics(alerts), [alerts])

  const addAlert = useCallback(async (alert) => {
    try {
      const response = await fetch('/api/alerts', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          event: alert.type,
          severity: alert.severity,
          affectedAsset: alert.target,
          detectionTime: new Date().toISOString(),
        }),
      })

      if (response.ok === false) {
        throw new Error(`Creating alert failed (${response.status}).`)
      }

      const createdAlert = await response.json()
      const [mappedAlert] = mapAlertData([createdAlert])

      setAlerts((currentAlerts) => [mappedAlert, ...currentAlerts])
      setError('')
    } catch (requestError) {
      setError(requestError.message)
    }
  }, [])

  const claimAlert = useCallback(async (alertId) => {
    setClaimNotice('')
    setClaimingAlertId(alertId)

    try {
      const response = await fetch(`/api/alerts/${alertId}/claim`, { method: 'POST' })

      if (response.status === 409) {
        const problem = await response.json()
        await refreshAlerts()
        setClaimNotice(problem.detail ?? 'This alert was just claimed by another user.')
        return
      }

      if (response.ok === false) {
        throw new Error(`Claiming alert failed (${response.status}).`)
      }

      const claimedAlert = await response.json()
      const [mappedAlert] = mapAlertData([claimedAlert])

      setAlerts((currentAlerts) => currentAlerts.map((alert) =>
        alert.id === mappedAlert.id ? mappedAlert : alert))
    } catch (requestError) {
      setClaimNotice(requestError.message)
    } finally {
      setClaimingAlertId(null)
    }
  }, [refreshAlerts])

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
    refreshAlerts,
    claimNotice,
    claimingAlertId,
  }
}