import { useQuery } from '@tanstack/react-query'
import { useId, useMemo, useState } from 'react'
import { getErrorMessage } from '../../../api/errors'
import { Alert } from '../../../components/Alert'
import { Spinner } from '../../../components/Spinner'
import { orderQueryOptions, useSetReception } from '../api'
import { ReceptionContext } from '../ReceptionContext'
import { hideIfReceived } from '../status'
import type { ReceptionTarget } from '../types'
import styles from './OrderDetail.module.css'
import { PalletItem } from './PalletItem'
import { ReceptionProgress } from './ReceptionProgress'
import { StatusBadge } from './StatusBadge'

export function OrderDetail({ orderId }: { orderId: string }) {
  const { data: order, error, isPending, refetch } = useQuery(orderQueryOptions(orderId))
  const { mutate, error: updateError, reset: dismissUpdateError } = useSetReception(orderId)
  const [hideReceived, setHideReceived] = useState(false)
  const headingId = useId()
  const filterId = useId()

  const context = useMemo(
    () => ({
      orderId,
      hideReceived,
      setReception: (target: ReceptionTarget, isReceived: boolean) =>
        mutate({ target, isReceived }),
    }),
    [orderId, hideReceived, mutate],
  )

  if (isPending) {
    return <Spinner label="Chargement de la commande…" />
  }

  if (error) {
    return (
      <Alert
        action={
          <button type="button" onClick={() => void refetch()}>
            Réessayer
          </button>
        }
      >
        {getErrorMessage(error)}
      </Alert>
    )
  }

  const pallets = hideIfReceived(order.pallets, hideReceived)

  return (
    <ReceptionContext value={context}>
      <section className={styles.detail} aria-labelledby={headingId}>
        <header className={styles.header}>
          <h2 id={headingId}>Commande {order.orderId}</h2>
          <StatusBadge status={order.status} />
        </header>

        <ReceptionProgress progress={order.progress} />

        <div className={styles.toolbar}>
          <input
            id={filterId}
            type="checkbox"
            checked={hideReceived}
            onChange={(event) => setHideReceived(event.target.checked)}
          />
          <label htmlFor={filterId}>Masquer les éléments reçus</label>
        </div>

        {updateError && (
          <Alert onDismiss={dismissUpdateError}>
            La mise à jour a échoué : {getErrorMessage(updateError)}
          </Alert>
        )}

        {pallets.length > 0 ? (
          <ul aria-label="Palettes">
            {pallets.map((pallet) => (
              <PalletItem key={pallet.palletId} pallet={pallet} />
            ))}
          </ul>
        ) : (
          <p className={styles.empty}>Tous les éléments de cette commande sont reçus.</p>
        )}
      </section>
    </ReceptionContext>
  )
}
