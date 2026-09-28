import type { ReactNode } from 'react'
import styles from './Alert.module.css'

interface AlertProps {
  children: ReactNode
  onDismiss?: () => void
  action?: ReactNode
}

/** An error message, announced immediately by screen readers (role="alert"). */
export function Alert({ children, onDismiss, action }: AlertProps) {
  return (
    <div role="alert" className={styles.alert}>
      <p className={styles.message}>{children}</p>
      {action}
      {onDismiss && (
        <button type="button" className={styles.dismiss} onClick={onDismiss} aria-label="Fermer">
          ×
        </button>
      )}
    </div>
  )
}
