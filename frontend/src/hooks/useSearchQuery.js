import { useState } from 'react'

export default function useSearchQuery() {
  const [query, setQuery] = useState('')

  return { query, setQuery }
}
