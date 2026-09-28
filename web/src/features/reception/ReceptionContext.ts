import { createContext, useContext } from 'react'
import type { ReceptionTarget } from './types'

interface ReceptionContextValue {
  orderId: string
  hideReceived: boolean
  setReception: (target: ReceptionTarget, isReceived: boolean) => void
}

/** Shared by every row of the order tree, instead of passing the same props through each level. */
export const ReceptionContext = createContext<ReceptionContextValue | null>(null)

export function useReceptionContext(): ReceptionContextValue {
  const context = useContext(ReceptionContext)
  if (!context) {
    throw new Error('useReceptionContext must be used inside <ReceptionContext>.')
  }
  return context
}
