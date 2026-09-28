import { setupServer } from 'msw/node'

/** Intercepts fetch() in the tests. Each test registers its handlers with server.use(...). */
export const server = setupServer()
