import { useCallback, useEffect, useState } from 'react'

import { fetchCases } from './api/casesApi'

export default function useCases() {
  const [cases, setCases] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [herlaadTeller, setHerlaadTeller] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    async function laad() {
      setLoading(true)
      setError('')
      try {
        setCases(await fetchCases(controller.signal))
      } catch (requestError) {
        if (requestError.name !== 'AbortError') setError(requestError.message)
      } finally {
        if (!controller.signal.aborted) setLoading(false)
      }
    }

    laad()
    return () => controller.abort()
  }, [herlaadTeller])

  const herlaad = useCallback(() => setHerlaadTeller((teller) => teller + 1), [])

  return { cases, loading, error, herlaad }
}
