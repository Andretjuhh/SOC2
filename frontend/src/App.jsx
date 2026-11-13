import AppRoutes from './AppRoutes'
import useAlerts from './features/alerts/useAlerts'
import useSearchQuery from './hooks/useSearchQuery'

function App() {
  const { query, setQuery } = useSearchQuery()
  const alerts = useAlerts(query)

  return <AppRoutes query={query} onQueryChange={setQuery} alerts={alerts} />
}

export default App
