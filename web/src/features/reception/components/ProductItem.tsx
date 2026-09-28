import { useId } from 'react'
import { statusFromValue } from '../status'
import type { ProductLine } from '../types'
import { ReceptionCheckbox } from './ReceptionCheckbox'
import styles from './Tree.module.css'

export function ProductItem({ product }: { product: ProductLine }) {
  const checkboxId = useId()

  return (
    <li className={`${styles.row} ${styles.product}`}>
      <ReceptionCheckbox
        id={checkboxId}
        target={{ level: 'product', productLineId: product.id }}
        status={statusFromValue(product.isReceived)}
      />
      {/* The whole row is the label: a large click target, handy on a warehouse tablet. */}
      <label htmlFor={checkboxId} className={`${styles.label} ${styles.productLabel}`}>
        <span className={styles.title}>{product.name}</span>
        <span className={styles.reference}>{product.ref}</span>
        <span className={styles.meta}>
          {product.color} · Taille {product.size}
        </span>
      </label>
      <span className={styles.quantity}>× {product.expectedQuantity}</span>
    </li>
  )
}
