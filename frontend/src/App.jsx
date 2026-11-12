import { useMemo, useState } from 'react'
import { Navigate, Outlet, Route, Routes } from 'react-router-dom'

import OverviewPage from './pages/OverviewPage'
import AlertsPage from './pages/AlertsPage'
import AppLayout from './layouts/AppLayout'
import LoginPage from './pages/LoginPage'
import ActivityPage from './pages/ActivityPage'
import TicketPage from './pages/TicketPage'
import CasesPage from './pages/CasesPage'
import AssetsPage from './pages/AssetsPage'

const events = [
  { time: '10:42:18', type: 'Unusual sign-in location', source: 'Identity Protection', severity: 'Critical', target: 'j.smith@acme.io', status: 'Investigating' },
  { time: '10:38:51', type: 'Multiple failed login attempts', source: 'Microsoft Entra ID', severity: 'High', target: '10.24.8.119', status: 'Open' },
  { time: '10:31:06', type: 'Suspicious PowerShell activity', source: 'Endpoint Detection', severity: 'High', target: 'WS-0421', status: 'Investigating' },
  { time: '10:24:33', type: 'New inbox forwarding rule', source: 'Email Security', severity: 'Medium', target: 'm.chen@acme.io', status: 'Open' },
  { time: '10:12:09', type: 'Connection to a new domain', source: 'Network Monitor', severity: 'Low', target: '10.24.3.54', status: 'Resolved' },
]

function App() {
  const [query, setQuery] = useState('')
  const [severityFilter, setSeverityFilter] = useState('All events')


  const filteredEvents = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase()

    return events.filter((event) => {
      const matchesSeverity = severityFilter === 'All events' || event.severity === severityFilter
      const matchesQuery = !normalizedQuery
        || `${event.type} ${event.source} ${event.target} ${event.severity}`.toLowerCase().includes(normalizedQuery)

      return matchesSeverity && matchesQuery
    })
  }, [query, severityFilter])

  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<LoginPage />} />

      <Route element={<AppLayout query={query} onQueryChange={setQuery}><Outlet /></AppLayout>}>
        <Route path="/overview" element={<OverviewPage /> } />
        <Route path="/alerts" element={<AlertsPage filteredEvents={filteredEvents} severityFilter={severityFilter} setSeverityFilter={setSeverityFilter} />} />
        <Route path="/activity" element={<ActivityPage />} />
        <Route path="/assets" element={<AssetsPage />} />
        <Route path="/tickets" element={<TicketPage />} />
        <Route path="/cases" element={<CasesPage />} />
      </Route>
    </Routes>
  )
}

export default App
