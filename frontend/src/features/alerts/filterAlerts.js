export function filterAlerts(alerts, query, severityFilter) {
  const normalizedQuery = query.trim().toLowerCase()

  return alerts.filter((alert) => {
    const matchesSeverity = severityFilter === 'All events' || alert.severity === severityFilter
    const matchesQuery = !normalizedQuery
      || `${alert.type} ${alert.target} ${alert.severity}`.toLowerCase().includes(normalizedQuery)

    return matchesSeverity && matchesQuery
  })
}
