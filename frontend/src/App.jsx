import { useEffect, useMemo, useState } from 'react'
import { Navigate, Outlet, Route, Routes } from 'react-router-dom'

import OverviewPage from './pages/OverviewPage'
import AlertsPage from './pages/AlertsPage'
import AppLayout from './layouts/AppLayout'
import LoginPage from './pages/LoginPage'
import ActivityPage from './pages/ActivityPage'
import TicketPage from './pages/TicketPage'
import CasesPage from './pages/CasesPage'
import AssetsPage from './pages/AssetsPage'
import { filterAlerts } from './features/alerts/filterAlerts'
import { getAlertMetrics } from './features/alerts/getAlertMetrics'

function App() {
  const [query, setQuery] = useState('')
  const [severityFilter, setSeverityFilter] = useState('All events')
  const [alerts, setAlerts] = useState([])
  const [alertsLoading, setAlertsLoading] = useState(true)
  const [alertsError, setAlertsError] = useState('')

  useEffect(() => {
    const controller = new AbortController()

    async function loadAlerts() {
      try {
        const response = await fetch('/api/alerts', { signal: controller.signal })
        if (!response.ok) {
          throw new Error(`Loading alerts failed (${response.status}).`)
        }

        const data = await response.json()
        setAlerts(data.map((alert) => ({
          id: alert.alertID,
          type: alert.event,
          severity: alert.severity,
          target: alert.affectedAsset,
          time: new Date(alert.detectionTime).toLocaleTimeString(),
          closedTime: alert.closedTime ? new Date(alert.closedTime).toLocaleString() : null,
        })))
      } catch (error) {
        if (error.name !== 'AbortError') {
          setAlertsError(error.message)
        }
      } finally {
        if (!controller.signal.aborted) {
          setAlertsLoading(false)
        }
      }
    }

    loadAlerts()
    return () => controller.abort()
  }, [])

  const filteredEvents = useMemo(
    () => filterAlerts(alerts, query, severityFilter),
    [alerts, query, severityFilter],
  )
  const alertMetrics = useMemo(() => getAlertMetrics(alerts), [alerts])

  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage />} />

      <Route element={<AppLayout query={query} onQueryChange={setQuery}><Outlet /></AppLayout>}>
        <Route path="/overview" element={<OverviewPage alertMetrics={alertMetrics} loading={alertsLoading} error={alertsError} />} />
        <Route path="/alerts" element={<AlertsPage filteredEvents={filteredEvents} totalEvents={alerts.length} loading={alertsLoading} error={alertsError} severityFilter={severityFilter} setSeverityFilter={setSeverityFilter} />} />
        <Route path="/activity" element={<ActivityPage />} />
        <Route path="/assets" element={<AssetsPage />} />
        <Route path="/tickets" element={<TicketPage />} />
        <Route path="/cases" element={<CasesPage />} />
      </Route>
    </Routes>
  )
}

export default App
