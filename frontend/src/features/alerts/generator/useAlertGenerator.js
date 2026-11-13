import { useEffect, useState } from 'react'

import { startAlertGenerator } from './alertGenerator'

export default function useAlertGenerator(templates, onAlert) {
  const [running, setRunning] = useState(true)
  const [intervalMs, setIntervalMs] = useState(4000)

  function changeSpeed(direction) {
    setIntervalMs((current) => Math.min(10000, Math.max(1000, current - direction * 1000)))
  }

  useEffect(() => {
    if (!running || templates.length === 0) {
      return undefined
    }

    return startAlertGenerator(templates, onAlert, intervalMs)
  }, [intervalMs, onAlert, running, templates])

  return { running, setRunning, intervalMs, changeSpeed }
}
