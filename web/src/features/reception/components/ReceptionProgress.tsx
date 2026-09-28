import type { Progress } from '../types'
import styles from './ReceptionProgress.module.css'

function percent(received: number, total: number): number {
  return total === 0 ? 0 : Math.round((received / total) * 100)
}

/** Global gauge of the order: "X / Y articles reçus" (units) and the number of product lines. */
export function ReceptionProgress({ progress }: { progress: Progress }) {
  const { receivedUnits, totalUnits, receivedLines, totalLines } = progress
  const value = percent(receivedUnits, totalUnits)
  const text = `${receivedUnits} / ${totalUnits} articles reçus`

  return (
    <div className={styles.progress}>
      <div className={styles.labels}>
        <p className={styles.main}>{text}</p>
        <p className={styles.secondary}>
          {receivedLines} / {totalLines} lignes produit · {value} %
        </p>
      </div>
      <div
        role="progressbar"
        aria-label="Avancement de la réception"
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={value}
        aria-valuetext={text}
        className={styles.track}
      >
        <div
          className={`${styles.fill} ${value === 100 ? styles.complete : ''}`}
          style={{ width: `${value}%` }}
        />
      </div>
    </div>
  )
}

/** Compact "x / y" counter shown on each pallet and carton row. */
export function ProgressCount({ progress }: { progress: Progress }) {
  return (
    <span className={styles.count}>
      {progress.receivedLines} / {progress.totalLines}
      <span className="visually-hidden"> produits reçus</span>
    </span>
  )
}
