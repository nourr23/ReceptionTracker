import type { components } from '../../api/schema'

// Friendly names for the generated API types: the API remains the single source of truth.
type Schemas = components['schemas']

export type OrderSummary = Schemas['OrderSummaryDto']
export type Order = Schemas['OrderDto']
export type Pallet = Schemas['PalletDto']
export type Carton = Schemas['CartonDto']
export type ProductLine = Schemas['ProductLineDto']
export type Progress = Schemas['ProgressDto']
export type ReceptionStatus = Schemas['ReceptionStatus']

/** What a validation checkbox acts on: one of the three levels of the hierarchy. */
export type ReceptionTarget =
  | { level: 'pallet'; palletId: string }
  | { level: 'carton'; palletId: string; cartonId: string }
  | { level: 'product'; productLineId: number }
