import type { ReceptionStatus } from './types'

export const statusLabels: Record<ReceptionStatus, string> = {
  Pending: 'En attente',
  PartiallyReceived: 'Partiellement reçu',
  Received: 'Reçu',
}

/** Received → checked, partially received → indeterminate, pending → unchecked. */
export function checkboxState(status: ReceptionStatus) {
  return {
    checked: status === 'Received',
    indeterminate: status === 'PartiallyReceived',
  }
}

/** Clicking a pending or partial element receives all of it; clicking a received one un-receives it. */
export function nextReceptionValue(status: ReceptionStatus): boolean {
  return status !== 'Received'
}

export function statusFromValue(isReceived: boolean): ReceptionStatus {
  return isReceived ? 'Received' : 'Pending'
}

export function hideIfReceived<T extends { status: ReceptionStatus }>(
  items: readonly T[],
  hideReceived: boolean,
): readonly T[] {
  return hideReceived ? items.filter((item) => item.status !== 'Received') : items
}
