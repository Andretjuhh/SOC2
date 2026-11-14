import { useCallback, useEffect, useState } from 'react'
import { startAlertGenerator } from './alertGenerator'

const MIN_INTERVAL_MS = 1
const MAX_INTERVAL_MS = 64000

export default function useAlertGenerator(templates, onAlert) {
  const [running, setRunning] = useState(false)
  const [intervalMs, setIntervalMs] = useState(2000)

  function changeSpeed(direction) {
    setIntervalMs((current) => {
      const nextInterval = direction === 'faster' ? current / 2 : current * 2
      return Math.min(MAX_INTERVAL_MS, Math.max(MIN_INTERVAL_MS, nextInterval))
    })
  }

  function toggleRunning() {
    setRunning((current) => !current)
  }

  const pause = useCallback(() => setRunning(false), [])

  useEffect(() => {
    if (!running || templates.length === 0) {
      return undefined
    }

    return startAlertGenerator(templates, onAlert, intervalMs)
  }, [intervalMs, onAlert, running, templates])

  const intervalLabel = intervalMs < 1000 ? `${intervalMs}ms` : `${intervalMs / 1000}s`

  return {
    running,
    toggleRunning,
    pause,
    intervalMs,
    intervalLabel,
    canSpeedUp: intervalMs > MIN_INTERVAL_MS,
    canSlowDown: intervalMs < MAX_INTERVAL_MS,
    changeSpeed,
  }
}
