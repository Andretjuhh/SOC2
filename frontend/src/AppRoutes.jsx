import { Navigate, Outlet, Route, Routes } from 'react-router-dom'

import ActivityPage from './pages/ActivityPage'
import AlertsPage from './pages/AlertsPage'
import AssetsPage from './pages/AssetsPage'
import CasesPage from './pages/CasesPage'
import LoginPage from './pages/LoginPage'
import OverviewPage from './pages/OverviewPage'
import TicketPage from './pages/TicketPage'
import AppLayout from './layouts/AppLayout'

function AppRoutes({ query, onQueryChange, alerts }) {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage />} />

      <Route element={<AppLayout query={query} onQueryChange={onQueryChange}><Outlet /></AppLayout>}>
        <Route path="/overview" element={<OverviewPage alertMetrics={alerts.metrics} loading={alerts.loading} error={alerts.error} testAlerts={alerts.testTemplates} onAlertGenerated={alerts.addAlert} />} />
        <Route path="/alerts" element={<AlertsPage filteredEvents={alerts.filteredEvents} totalEvents={alerts.alerts.length} loading={alerts.loading} error={alerts.error} severityFilter={alerts.severityFilter} setSeverityFilter={alerts.setSeverityFilter} />} />
        <Route path="/activity" element={<ActivityPage />} />
        <Route path="/assets" element={<AssetsPage />} />
        <Route path="/tickets" element={<TicketPage />} />
        <Route path="/cases" element={<CasesPage />} />
      </Route>
    </Routes>
  )
}

export default AppRoutes
