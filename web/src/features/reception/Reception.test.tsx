import { screen, waitFor, within } from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { createFakeReceptionApi } from '../../test/fakeReceptionApi'
import { renderApp } from '../../test/renderApp'
import { server } from '../../test/server'

function mockApi(options?: Parameters<typeof createFakeReceptionApi>[0]) {
  const api = createFakeReceptionApi(options)
  server.use(...api.handlers)
  return api
}

const checkbox = (name: RegExp) => screen.getByRole('checkbox', { name })
const progressBar = () => screen.getByRole('progressbar', { name: 'Avancement de la réception' })

async function renderOrder() {
  const result = renderApp()
  await screen.findByRole('heading', { name: 'Commande CMD-2026' })
  return result
}

describe('Reception', () => {
  it('opens the first pending order with its global progress', async () => {
    mockApi()

    await renderOrder()

    expect(progressBar()).toHaveAttribute('aria-valuetext', '0 / 120 articles reçus')
    expect(screen.getByText('0 / 4 lignes produit · 0 %')).toBeInTheDocument()
    expect(checkbox(/Palette PAL-01/)).not.toBeChecked()
  })

  it('shows pallets and cartons, and products only on demand', async () => {
    mockApi()
    const { user } = await renderOrder()

    expect(checkbox(/Carton CART-01-A/)).toBeVisible()
    expect(screen.queryByRole('checkbox', { name: /TSH-RED-M/ })).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Déplier Carton CART-01-A' }))

    expect(checkbox(/TSH-RED-M/)).toBeVisible()
  })

  it('validating a carton receives its products and makes its pallet partial', async () => {
    const api = mockApi()
    const { user } = await renderOrder()

    await user.click(checkbox(/Carton CART-01-A/))

    await waitFor(() => expect(checkbox(/Carton CART-01-A/)).toBeChecked())
    expect(checkbox(/Palette PAL-01/)).toBePartiallyChecked()
    expect(progressBar()).toHaveAttribute('aria-valuetext', '60 / 120 articles reçus')
    expect(api.requests).toEqual([
      { url: '/api/orders/CMD-2026/pallets/PAL-01/cartons/CART-01-A/reception', isReceived: true },
    ])
  })

  it('validating a pallet receives all its cartons', async () => {
    mockApi()
    const { user } = await renderOrder()

    await user.click(checkbox(/Palette PAL-01/))

    await waitFor(() => expect(checkbox(/Palette PAL-01/)).toBeChecked())
    expect(checkbox(/Carton CART-01-A/)).toBeChecked()
    expect(checkbox(/Carton CART-01-B/)).toBeChecked()
    expect(checkbox(/Palette PAL-02/)).not.toBeChecked()
  })

  it('validating every product of a carton one by one marks the carton as received', async () => {
    mockApi()
    const { user } = await renderOrder()
    await user.click(screen.getByRole('button', { name: 'Déplier Carton CART-01-A' }))

    await user.click(checkbox(/TSH-RED-M/))
    await waitFor(() => expect(checkbox(/Carton CART-01-A/)).toBePartiallyChecked())

    await user.click(checkbox(/SHO-BLK-42/))
    await waitFor(() => expect(checkbox(/Carton CART-01-A/)).toBeChecked())
  })

  it('unchecking a product puts its carton and pallet back to partially received', async () => {
    const api = mockApi({ receivedProductIds: [1, 2, 3] })
    const { user } = await renderOrder()
    expect(checkbox(/Palette PAL-01/)).toBeChecked()
    await user.click(screen.getByRole('button', { name: 'Déplier Carton CART-01-A' }))

    await user.click(checkbox(/TSH-RED-M/))

    await waitFor(() => expect(checkbox(/Carton CART-01-A/)).toBePartiallyChecked())
    expect(checkbox(/Palette PAL-01/)).toBePartiallyChecked()
    expect(api.requests).toEqual([
      { url: '/api/orders/CMD-2026/products/1/reception', isReceived: false },
    ])
  })

  it('clicking a partially received pallet receives all of it', async () => {
    const api = mockApi({ receivedProductIds: [1] })
    const { user } = await renderOrder()
    expect(checkbox(/Palette PAL-01/)).toBePartiallyChecked()

    await user.click(checkbox(/Palette PAL-01/))

    await waitFor(() => expect(checkbox(/Palette PAL-01/)).toBeChecked())
    expect(api.requests[0]?.isReceived).toBe(true)
  })

  it('can hide the elements already received', async () => {
    mockApi({ receivedProductIds: [1, 2] })
    const { user } = await renderOrder()
    expect(checkbox(/Carton CART-01-A/)).toBeInTheDocument()

    await user.click(screen.getByRole('checkbox', { name: 'Masquer les éléments reçus' }))

    expect(screen.queryByRole('checkbox', { name: /Carton CART-01-A/ })).not.toBeInTheDocument()
    expect(checkbox(/Carton CART-01-B/)).toBeInTheDocument()
  })

  it('shows an error when an update fails, and keeps the previous state', async () => {
    mockApi()
    server.use(
      http.put('*/api/orders/:orderId/pallets/:palletId/reception', () =>
        HttpResponse.json(
          { title: 'Not Found', status: 404, detail: "Pallet 'PAL-01' was not found." },
          { status: 404 },
        ),
      ),
    )
    const { user } = await renderOrder()

    await user.click(checkbox(/Palette PAL-01/))

    expect(await screen.findByRole('alert')).toHaveTextContent(
      "La mise à jour a échoué : Pallet 'PAL-01' was not found.",
    )
    expect(checkbox(/Palette PAL-01/)).not.toBeChecked()
  })

  it('switches to another order from the list and keeps it in the URL', async () => {
    mockApi()
    const { user } = await renderOrder()
    const orderList = screen.getByRole('navigation', { name: 'Commandes' })

    await user.click(within(orderList).getByRole('button', { name: /CMD-2027/ }))

    expect(await screen.findByRole('heading', { name: 'Commande CMD-2027' })).toBeInTheDocument()
    expect(window.location.search).toBe('?order=CMD-2027')
  })

  it('explains the problem when the server cannot be reached', async () => {
    server.use(http.get('*/api/orders', () => HttpResponse.error()))

    renderApp()

    expect(await screen.findByRole('alert')).toHaveTextContent('Impossible de contacter le serveur')
  })
})
