import styles from './ExpandButton.module.css'

interface ExpandButtonProps {
  expanded: boolean
  controls: string
  label: string
  onToggle: () => void
}

/** Disclosure button: aria-expanded / aria-controls tell assistive technologies what it opens. */
export function ExpandButton({ expanded, controls, label, onToggle }: ExpandButtonProps) {
  return (
    <button
      type="button"
      className={styles.button}
      aria-expanded={expanded}
      aria-controls={controls}
      aria-label={`${expanded ? 'Replier' : 'Déplier'} ${label}`}
      onClick={onToggle}
    >
      <span className={`${styles.chevron} ${expanded ? styles.expanded : ''}`} aria-hidden="true">
        ▸
      </span>
    </button>
  )
}
