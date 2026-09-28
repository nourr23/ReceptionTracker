import { useQuery } from '@tanstack/react-query'
import styles from './App.module.css'
import { getErrorMessage } from './api/errors'
import { Alert } from './components/Alert'
import { Spinner } from './components/Spinner'
import { ordersQueryOptions } from './features/reception/api'
import { OrderDetail } from './features/reception/components/OrderDetail'
import { OrderPicker } from './features/reception/components/OrderPicker'
import { useSearchParam } from './hooks/useSearchParam'

export function App() {
  const { data: orders, error, isPending, refetch } = useQuery(ordersQueryOptions())
  const [selectedOrderId, setSelectedOrderId] = useSearchParam('order')

  // Without a choice in the URL, open the first order that still has goods to receive.
  const orderId =
    selectedOrderId ??
    (orders?.find((order) => order.status !== 'Received') ?? orders?.[0])?.orderId

  return (
    <div className={styles.app}>
      <header className={styles.header}>
        <h1>Réception marchandises</h1>
      </header>

      <main className={styles.layout}>
        <aside className={styles.sidebar}>
          <h2 className={styles.sidebarTitle}>Commandes fournisseurs</h2>
          {isPending && <Spinner />}
          {error && (
            <Alert
              action={
                <button type="button" onClick={() => void refetch()}>
                  Réessayer
                </button>
              }
            >
              {getErrorMessage(error)}
            </Alert>
          )}
          {orders && (
            <OrderPicker orders={orders} selectedOrderId={orderId} onSelect={setSelectedOrderId} />
          )}
        </aside>

        <div className={styles.content}>
          {orderId ? (
            // key: switching order resets the local state (filters, expanded rows).
            <OrderDetail key={orderId} orderId={orderId} />
          ) : (
            orders && <p>Aucune commande à réceptionner.</p>
          )}
        </div>
      </main>
    </div>
  )
}
