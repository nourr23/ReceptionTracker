import { http, HttpResponse } from 'msw'
import type {
  Order,
  OrderSummary,
  ProductLine,
  Progress,
  ReceptionStatus,
} from '../features/reception/types'

// A small in-memory implementation of the API contract, used by the UI tests.
// Only product lines store their state; statuses and progress are derived, like on the server.

interface FakeOrder {
  orderId: string
  pallets: {
    palletId: string
    cartons: { cartonId: string; products: Omit<ProductLine, 'isReceived'>[] }[]
  }[]
}

const product = (
  id: number,
  ref: string,
  name: string,
  color: string,
  size: string,
  expectedQuantity: number,
) => ({ id, ref, name, color, size, expectedQuantity })

// CMD-2026: 4 product lines, 120 units.
// PAL-01 ─┬─ CART-01-A: #1 TSH-RED-M (50), #2 SHO-BLK-42 (10)
//         └─ CART-01-B: #3 TSH-RED-L (40)
// PAL-02 ─── CART-02-A: #4 BAL-FOOT-5 (20)
const orders: FakeOrder[] = [
  {
    orderId: 'CMD-2026',
    pallets: [
      {
        palletId: 'PAL-01',
        cartons: [
          {
            cartonId: 'CART-01-A',
            products: [
              product(1, 'TSH-RED-M', 'T-Shirt Sport', 'Rouge', 'M', 50),
              product(2, 'SHO-BLK-42', 'Baskets Running', 'Noir', '42', 10),
            ],
          },
          {
            cartonId: 'CART-01-B',
            products: [product(3, 'TSH-RED-L', 'T-Shirt Sport', 'Rouge', 'L', 40)],
          },
        ],
      },
      {
        palletId: 'PAL-02',
        cartons: [
          {
            cartonId: 'CART-02-A',
            products: [product(4, 'BAL-FOOT-5', 'Ballon de Football', 'Blanc', '5', 20)],
          },
        ],
      },
    ],
  },
  {
    orderId: 'CMD-2027',
    pallets: [
      {
        palletId: 'PAL-01',
        cartons: [
          {
            cartonId: 'CART-01-A',
            products: [product(10, 'RAQ-TEN-L2', 'Raquette de Tennis', 'Jaune', 'L2', 6)],
          },
        ],
      },
    ],
  },
]

export interface ReceptionRequest {
  url: string
  isReceived: boolean
}

export function createFakeReceptionApi({ receivedProductIds = [] as number[] } = {}) {
  const received = new Set(receivedProductIds)
  const requests: ReceptionRequest[] = []

  function progressOf(products: { id: number; expectedQuantity: number }[]): Progress {
    const done = products.filter((p) => received.has(p.id))
    const sum = (items: typeof products) =>
      items.reduce((total, p) => total + p.expectedQuantity, 0)
    return {
      receivedLines: done.length,
      totalLines: products.length,
      receivedUnits: sum(done),
      totalUnits: sum(products),
    }
  }

  function statusOf(progress: Progress): ReceptionStatus {
    if (progress.receivedLines === 0) return 'Pending'
    return progress.receivedLines === progress.totalLines ? 'Received' : 'PartiallyReceived'
  }

  const productsOf = (order: FakeOrder) =>
    order.pallets.flatMap((p) => p.cartons.flatMap((c) => c.products))

  function toDto(order: FakeOrder): Order {
    const progress = progressOf(productsOf(order))
    return {
      orderId: order.orderId,
      status: statusOf(progress),
      progress,
      pallets: order.pallets.map((pallet) => {
        const palletProgress = progressOf(pallet.cartons.flatMap((c) => c.products))
        return {
          palletId: pallet.palletId,
          status: statusOf(palletProgress),
          progress: palletProgress,
          cartons: pallet.cartons.map((carton) => {
            const cartonProgress = progressOf(carton.products)
            return {
              cartonId: carton.cartonId,
              status: statusOf(cartonProgress),
              progress: cartonProgress,
              products: carton.products.map((p) => ({ ...p, isReceived: received.has(p.id) })),
            }
          }),
        }
      }),
    }
  }

  const toSummary = (order: FakeOrder): OrderSummary => {
    const { orderId, status, progress } = toDto(order)
    return { orderId, status, progress }
  }

  const notFound = (code: string, detail: string) =>
    HttpResponse.json(
      { title: 'Not Found', status: 404, detail, code },
      { status: 404, headers: { 'Content-Type': 'application/problem+json' } },
    )

  async function applyReception(
    request: Request,
    order: FakeOrder,
    products: { id: number }[],
  ): Promise<Response> {
    const { isReceived } = (await request.json()) as { isReceived: boolean }
    requests.push({ url: new URL(request.url).pathname, isReceived })
    for (const p of products) {
      if (isReceived) received.add(p.id)
      else received.delete(p.id)
    }
    return HttpResponse.json(toDto(order))
  }

  const findOrder = (orderId: unknown) => orders.find((o) => o.orderId === orderId)

  const handlers = [
    http.get('*/api/orders', () => HttpResponse.json(orders.map(toSummary))),

    http.get('*/api/orders/:orderId', ({ params }) => {
      const order = findOrder(params.orderId)
      return order
        ? HttpResponse.json(toDto(order))
        : notFound('Order.NotFound', `Order '${String(params.orderId)}' was not found.`)
    }),

    http.put('*/api/orders/:orderId/pallets/:palletId/reception', ({ params, request }) => {
      const order = findOrder(params.orderId)
      const pallet = order?.pallets.find((p) => p.palletId === params.palletId)
      if (!order || !pallet) return notFound('Pallet.NotFound', 'Pallet not found.')
      return applyReception(
        request,
        order,
        pallet.cartons.flatMap((c) => c.products),
      )
    }),

    http.put(
      '*/api/orders/:orderId/pallets/:palletId/cartons/:cartonId/reception',
      ({ params, request }) => {
        const order = findOrder(params.orderId)
        const carton = order?.pallets
          .find((p) => p.palletId === params.palletId)
          ?.cartons.find((c) => c.cartonId === params.cartonId)
        if (!order || !carton) return notFound('Carton.NotFound', 'Carton not found.')
        return applyReception(request, order, carton.products)
      },
    ),

    http.put('*/api/orders/:orderId/products/:productLineId/reception', ({ params, request }) => {
      const order = findOrder(params.orderId)
      const line = order && productsOf(order).find((p) => p.id === Number(params.productLineId))
      if (!order || !line) return notFound('ProductLine.NotFound', 'Product line not found.')
      return applyReception(request, order, [line])
    }),
  ]

  return { handlers, requests }
}
