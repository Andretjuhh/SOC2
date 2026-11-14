import AppRoutes from './AppRoutes'
import useSearchQuery from './hooks/useSearchQuery'

function App() {
  const { query, setQuery } = useSearchQuery()

  return <AppRoutes query={query} onQueryChange={setQuery} />

}

export default App
