import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { TriStateCheckbox } from './TriStateCheckbox'

describe('TriStateCheckbox', () => {
  it('is partially checked when indeterminate', () => {
    render(<TriStateCheckbox aria-label="box" checked={false} indeterminate readOnly />)

    expect(screen.getByRole('checkbox', { name: 'box' })).toBePartiallyChecked()
  })

  it('leaves the indeterminate state when the prop changes', () => {
    const { rerender } = render(
      <TriStateCheckbox aria-label="box" checked={false} indeterminate readOnly />,
    )

    rerender(<TriStateCheckbox aria-label="box" checked indeterminate={false} readOnly />)

    const box = screen.getByRole('checkbox', { name: 'box' })
    expect(box).not.toBePartiallyChecked()
    expect(box).toBeChecked()
  })
})
