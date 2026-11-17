import { useState } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'

import ActivityPage from './pages/ActivityPage'
import AlertsPage from './pages/AlertsPage'
import AppLayout from './layouts/AppLayout'
import AssetsPage from './pages/AssetsPage'
import CasesPage from './pages/CasesPage'
import OverviewPage from './pages/OverviewPage'
import TicketPage from './pages/TicketPage'

function App() {
  const [query, setQuery] = useState('')

  return (
    <AppLayout query={query} onQueryChange={setQuery}>
      <Routes>
        <Route path="/" element={<Navigate to="/overview" replace />} />
        <Route path="/overview" element={<OverviewPage />} />
        <Route
          path="/alerts"
          element={(
            <AlertsPage
              onQueryChange={setQuery}
              query={query}
            />
          )}
        />
        <Route path="/activity" element={<ActivityPage />} />
        <Route path="/assets" element={<AssetsPage />} />
        <Route path="/tickets" element={<TicketPage />} />
        <Route path="/cases" element={<CasesPage />} />
      </Routes>
    </AppLayout>
  )
}

export default App
