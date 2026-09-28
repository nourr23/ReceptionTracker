import { TriStateCheckbox } from '../../../components/TriStateCheckbox'
import { usePendingReception } from '../api'
import { useReceptionContext } from '../ReceptionContext'
import { checkboxState, nextReceptionValue, statusFromValue } from '../status'
import type { ReceptionStatus, ReceptionTarget } from '../types'

interface ReceptionCheckboxProps {
  id: string
  target: ReceptionTarget
  status: ReceptionStatus
}

/**
 * The validation checkbox of a pallet, a carton or a product line.
 * While its update is in flight it already shows the requested value (instant feedback)
 * and is disabled; the server's answer then becomes the source of truth.
 */
export function ReceptionCheckbox({ id, target, status }: ReceptionCheckboxProps) {
  const { orderId, setReception } = useReceptionContext()
  const pendingValue = usePendingReception(orderId, target)
  const isPending = pendingValue !== undefined

  const displayedStatus = isPending ? statusFromValue(pendingValue) : status
  const { checked, indeterminate } = checkboxState(displayedStatus)

  return (
    <TriStateCheckbox
      id={id}
      checked={checked}
      indeterminate={indeterminate}
      disabled={isPending}
      aria-busy={isPending}
      onChange={() => setReception(target, nextReceptionValue(status))}
    />
  )
}
