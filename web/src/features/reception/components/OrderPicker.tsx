import type { OrderSummary } from '../types'
import styles from './OrderPicker.module.css'
import { StatusBadge } from './StatusBadge'

interface OrderPickerProps {
  orders: readonly OrderSummary[]
  selectedOrderId: string | undefined
  onSelect: (orderId: string) => void
}

export function OrderPicker({ orders, selectedOrderId, onSelect }: OrderPickerProps) {
  return (
    <nav aria-label="Commandes">
      <ul className={styles.list}>
        {orders.map((order) => {
          const { receivedLines, totalLines } = order.progress
          return (
            <li key={order.orderId}>
              <button
                type="button"
                className={styles.order}
                aria-current={order.orderId === selectedOrderId ? 'true' : undefined}
                onClick={() => onSelect(order.orderId)}
              >
                <span className={styles.reference}>{order.orderId}</span>
                <StatusBadge status={order.status} />
                <span className={styles.progress}>
                  {receivedLines} / {totalLines} produits
                </span>
              </button>
            </li>
          )
        })}
      </ul>
    </nav>
  )
}
