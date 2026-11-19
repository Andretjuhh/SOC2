const severities = ['Critical', 'High', 'Medium', 'Low']

export function getAlertMetrics(alerts) {
  const activeAlerts = alerts.filter((alert) => !alert.closedTime)
  const severityCounts = Object.fromEntries(severities.map((severity) => [severity, 0]))

  for (const alert of activeAlerts) {
    if (Object.hasOwn(severityCounts, alert.severity)) {
      severityCounts[alert.severity] += 1
    }
  }

  return {
    activeCount: activeAlerts.length,
    severityCounts,
  }
}
