import { statusLabels } from '../status'
import type { ReceptionStatus } from '../types'
import styles from './StatusBadge.module.css'

export function StatusBadge({ status }: { status: ReceptionStatus }) {
  return <span className={`${styles.badge} ${styles[status]}`}>{statusLabels[status]}</span>
}
