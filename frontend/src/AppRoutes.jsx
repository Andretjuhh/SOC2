import { Navigate, Outlet, Route, Routes } from 'react-router-dom'

import ActivityPage from './pages/ActivityPage'
import AlertsPage from './pages/AlertsPage'
import AssetsPage from './pages/AssetsPage'
import CasesPage from './pages/CasesPage'
import LoginPage from './pages/LoginPage'
import OverviewPage from './pages/OverviewPage'
import TicketPage from './pages/TicketPage'
import AppLayout from './layouts/AppLayout'

import AlertsProvider from './features/alerts/AlertsProvider'

function AppRoutes({ query, onQueryChange }) {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage />} />

      <Route element={<AlertsProvider query={query}><Outlet /></AlertsProvider>}>
        <Route element={<AppLayout query={query} onQueryChange={onQueryChange}><Outlet /></AppLayout>}>
          <Route path="/overview" element={<OverviewPage />} />
          <Route path="/alerts" element={<AlertsPage />} />
          <Route path="/activity" element={<ActivityPage />} />
          <Route path="/assets" element={<AssetsPage />} />
          <Route path="/tickets" element={<TicketPage />} />
          <Route path="/cases" element={<CasesPage />} />
        </Route>
      </Route>
    </Routes>
  )
}

export default AppRoutes
