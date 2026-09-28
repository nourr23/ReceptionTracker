import { useEffect, useRef, type ComponentProps } from 'react'

type TriStateCheckboxProps = Omit<ComponentProps<'input'>, 'type' | 'ref'> & {
  indeterminate?: boolean
}

/**
 * A native checkbox that can also be "indeterminate" (partially checked).
 * `indeterminate` only exists as a DOM property, not as an HTML attribute,
 * so it has to be set through a ref. Screen readers announce it as "mixed".
 */
export function TriStateCheckbox({ indeterminate = false, ...props }: TriStateCheckboxProps) {
  const ref = useRef<HTMLInputElement>(null)

  // Runs after every render: a click resets the DOM flag, so it must be re-applied each time.
  useEffect(() => {
    if (ref.current) {
      ref.current.indeterminate = indeterminate
    }
  })

  return <input ref={ref} type="checkbox" {...props} />
}
