let nextGeneratedId = 0

export function startAlertGenerator(templates, onAlert, intervalMs) {
  if (templates.length === 0) {
    return () => {}
  }

  const timerId = window.setInterval(() => {
    const template = templates[Math.floor(Math.random() * templates.length)]
    const now = new Date()

    onAlert({
      ...template,
      id: `generated-${nextGeneratedId++}`,
      status: 'New',
      time: now.toLocaleTimeString(),
      closedTime: null,
    })
  }, intervalMs)

  return () => window.clearInterval(timerId)
}
