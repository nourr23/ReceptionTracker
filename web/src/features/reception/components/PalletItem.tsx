import { useId, useState } from 'react'
import { ExpandButton } from '../../../components/ExpandButton'
import { useReceptionContext } from '../ReceptionContext'
import { hideIfReceived } from '../status'
import type { Pallet } from '../types'
import { CartonItem } from './CartonItem'
import { ProgressCount } from './ReceptionProgress'
import { ReceptionCheckbox } from './ReceptionCheckbox'
import { StatusBadge } from './StatusBadge'
import styles from './Tree.module.css'

export function PalletItem({ pallet }: { pallet: Pallet }) {
  const { hideReceived } = useReceptionContext()
  // Pallets start expanded (their cartons are visible), cartons start collapsed:
  // the products are only shown on demand, which keeps the screen light.
  const [expanded, setExpanded] = useState(true)
  const checkboxId = useId()
  const contentId = useId()
  const cartons = hideIfReceived(pallet.cartons, hideReceived)
  const title = `Palette ${pallet.palletId}`

  return (
    <li className={styles.pallet}>
      <div className={`${styles.row} ${styles.palletRow}`}>
        <ExpandButton
          expanded={expanded}
          controls={contentId}
          label={title}
          onToggle={() => setExpanded((value) => !value)}
        />
        <ReceptionCheckbox
          id={checkboxId}
          target={{ level: 'pallet', palletId: pallet.palletId }}
          status={pallet.status}
        />
        <label htmlFor={checkboxId} className={styles.label}>
          <span className={styles.title}>{title}</span>
          <span className={styles.meta}>
            {pallet.cartons.length} carton{pallet.cartons.length > 1 ? 's' : ''}
          </span>
        </label>
        <ProgressCount progress={pallet.progress} />
        <span className={styles.status}>
          <StatusBadge status={pallet.status} />
        </span>
      </div>

      <ul id={contentId} hidden={!expanded} className={styles.children}>
        {cartons.map((carton) => (
          <CartonItem key={carton.cartonId} palletId={pallet.palletId} carton={carton} />
        ))}
      </ul>
    </li>
  )
}
