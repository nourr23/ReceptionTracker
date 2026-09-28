import { useId, useState } from 'react'
import { ExpandButton } from '../../../components/ExpandButton'
import { useReceptionContext } from '../ReceptionContext'
import type { Carton } from '../types'
import { ProductItem } from './ProductItem'
import { ProgressCount } from './ReceptionProgress'
import { ReceptionCheckbox } from './ReceptionCheckbox'
import { StatusBadge } from './StatusBadge'
import styles from './Tree.module.css'

interface CartonItemProps {
  palletId: string
  carton: Carton
}

export function CartonItem({ palletId, carton }: CartonItemProps) {
  const { hideReceived } = useReceptionContext()
  const [expanded, setExpanded] = useState(false)
  const checkboxId = useId()
  const contentId = useId()
  const products = hideReceived ? carton.products.filter((p) => !p.isReceived) : carton.products
  const title = `Carton ${carton.cartonId}`

  return (
    <li className={styles.carton}>
      <div className={styles.row}>
        <ExpandButton
          expanded={expanded}
          controls={contentId}
          label={title}
          onToggle={() => setExpanded((value) => !value)}
        />
        <ReceptionCheckbox
          id={checkboxId}
          target={{ level: 'carton', palletId, cartonId: carton.cartonId }}
          status={carton.status}
        />
        <label htmlFor={checkboxId} className={styles.label}>
          <span className={styles.title}>{title}</span>
          <span className={styles.meta}>
            {carton.products.length} produit{carton.products.length > 1 ? 's' : ''}
          </span>
        </label>
        <ProgressCount progress={carton.progress} />
        <span className={styles.status}>
          <StatusBadge status={carton.status} />
        </span>
      </div>

      <ul id={contentId} hidden={!expanded} className={styles.children}>
        {products.map((product) => (
          <ProductItem key={product.id} product={product} />
        ))}
      </ul>
    </li>
  )
}
