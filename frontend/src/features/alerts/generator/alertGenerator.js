let nextGeneratedId = 0

export function startAlertGenerator(templates, onAlert, intervalMs) {
  if (templates.length === 0) {
    return () => {}
  }

  nextGeneratedId = Math.max(nextGeneratedId, ...templates.map((alert) => alert.id)) + 1
  const timerId = window.setInterval(() => {
    const template = templates[Math.floor(Math.random() * templates.length)]
    const now = new Date()

    onAlert({
      ...template,
      id: nextGeneratedId++,
      status: 'New',
      time: now.toLocaleTimeString(),
      closedTime: null,
    })
  }, intervalMs)

  return () => window.clearInterval(timerId)
}
