import { queryOptions, useMutation, useMutationState, useQueryClient } from '@tanstack/react-query'
import { apiClient, unwrap } from '../../api/client'
import type { Order, ReceptionTarget } from './types'

// One place for the cache keys, so reads and invalidations can never get out of sync.
export const receptionKeys = {
  all: ['orders'] as const,
  list: () => [...receptionKeys.all, 'list'] as const,
  detail: (orderId: string) => [...receptionKeys.all, 'detail', orderId] as const,
  update: (orderId: string) => [...receptionKeys.all, 'update', orderId] as const,
}

export const ordersQueryOptions = () =>
  queryOptions({
    queryKey: receptionKeys.list(),
    queryFn: ({ signal }) => unwrap(apiClient.GET('/api/orders', { signal })),
  })

export const orderQueryOptions = (orderId: string) =>
  queryOptions({
    queryKey: receptionKeys.detail(orderId),
    queryFn: ({ signal }) =>
      unwrap(apiClient.GET('/api/orders/{orderId}', { params: { path: { orderId } }, signal })),
  })

function putReception(orderId: string, target: ReceptionTarget, isReceived: boolean) {
  const body = { isReceived }

  switch (target.level) {
    case 'pallet':
      return unwrap(
        apiClient.PUT('/api/orders/{orderId}/pallets/{palletId}/reception', {
          params: { path: { orderId, palletId: target.palletId } },
          body,
        }),
      )
    case 'carton':
      return unwrap(
        apiClient.PUT('/api/orders/{orderId}/pallets/{palletId}/cartons/{cartonId}/reception', {
          params: { path: { orderId, palletId: target.palletId, cartonId: target.cartonId } },
          body,
        }),
      )
    case 'product':
      return unwrap(
        apiClient.PUT('/api/orders/{orderId}/products/{productLineId}/reception', {
          params: { path: { orderId, productLineId: target.productLineId } },
          body,
        }),
      )
  }
}

export interface SetReceptionVariables {
  target: ReceptionTarget
  isReceived: boolean
}

/**
 * Validates / un-validates a pallet, a carton or a product line.
 * The API answers with the whole refreshed order, which replaces the cached one:
 * the statuses of every parent and the progress are always the server's.
 */
export function useSetReception(orderId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationKey: receptionKeys.update(orderId),
    mutationFn: ({ target, isReceived }: SetReceptionVariables) =>
      putReception(orderId, target, isReceived),
    // Updates of the same order run one after the other, so an older response
    // can never arrive last and overwrite a newer state.
    scope: { id: `reception-${orderId}` },
    onSuccess: (order: Order) => {
      queryClient.setQueryData(receptionKeys.detail(orderId), order)
      // The list shows each order's progress: refresh it in the background.
      void queryClient.invalidateQueries({ queryKey: receptionKeys.list() })
    },
  })
}

/** The value a checkbox is being set to while its update is in flight, if any. */
export function usePendingReception(orderId: string, target: ReceptionTarget): boolean | undefined {
  const pending = useMutationState({
    filters: { mutationKey: receptionKeys.update(orderId), status: 'pending' },
    select: (mutation) => mutation.state.variables as SetReceptionVariables | undefined,
  })

  // The most recent click on this element wins.
  return pending.findLast((variables) => variables && isSameTarget(variables.target, target))
    ?.isReceived
}

function isSameTarget(a: ReceptionTarget, b: ReceptionTarget): boolean {
  switch (a.level) {
    case 'pallet':
      return b.level === 'pallet' && a.palletId === b.palletId
    case 'carton':
      return b.level === 'carton' && a.palletId === b.palletId && a.cartonId === b.cartonId
    case 'product':
      return b.level === 'product' && a.productLineId === b.productLineId
  }
}
