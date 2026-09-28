import { useCallback, useEffect, useState } from 'react'

function readParam(name: string): string | undefined {
  return new URLSearchParams(window.location.search).get(name) ?? undefined
}

/**
 * State stored in the URL query string (e.g. ?order=CMD-2026):
 * it survives a page reload, can be shared as a link, and follows the browser's back/forward buttons.
 */
export function useSearchParam(name: string) {
  const [value, setValue] = useState(() => readParam(name))

  useEffect(() => {
    const onPopState = () => setValue(readParam(name))
    window.addEventListener('popstate', onPopState)
    return () => window.removeEventListener('popstate', onPopState)
  }, [name])

  const update = useCallback(
    (next: string) => {
      const url = new URL(window.location.href)
      url.searchParams.set(name, next)
      window.history.pushState(null, '', url)
      setValue(next)
    },
    [name],
  )

  return [value, update] as const
}
