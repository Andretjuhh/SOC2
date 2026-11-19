export const ALERT_POLL_INTERVAL_MS = 10_000

export function startAlertPolling(refreshAlerts) {
  let timeoutId
  let requestController
  let requestInProgress = false
  let stopped = false

  const scheduleNextPoll = () => {
    if (!stopped && document.visibilityState === 'visible') {
      timeoutId = window.setTimeout(poll, ALERT_POLL_INTERVAL_MS)
    }
  }

  const poll = async () => {
    if (stopped || document.visibilityState !== 'visible' || requestInProgress) {
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
      poll()
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
    stopped = true
    window.clearTimeout(timeoutId)
    requestController?.abort()
    window.removeEventListener('online', refreshImmediately)
    document.removeEventListener('visibilitychange', handleVisibilityChange)
  }
}
