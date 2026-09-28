import { describe, expect, it } from 'vitest'
import { checkboxState, hideIfReceived, nextReceptionValue } from './status'

describe('checkboxState', () => {
  it.each([
    ['Pending', { checked: false, indeterminate: false }],
    ['PartiallyReceived', { checked: false, indeterminate: true }],
    ['Received', { checked: true, indeterminate: false }],
  ] as const)('maps %s', (status, expected) => {
    expect(checkboxState(status)).toEqual(expected)
  })
})

describe('nextReceptionValue', () => {
  it('receives everything from a pending or partial element', () => {
    expect(nextReceptionValue('Pending')).toBe(true)
    expect(nextReceptionValue('PartiallyReceived')).toBe(true)
  })

  it('un-receives a received element', () => {
    expect(nextReceptionValue('Received')).toBe(false)
  })
})

describe('hideIfReceived', () => {
  const items = [
    { id: 1, status: 'Received' as const },
    { id: 2, status: 'PartiallyReceived' as const },
  ]

  it('keeps everything when the filter is off', () => {
    expect(hideIfReceived(items, false)).toEqual(items)
  })

  it('removes received items when the filter is on', () => {
    expect(hideIfReceived(items, true)).toEqual([items[1]])
  })
})
