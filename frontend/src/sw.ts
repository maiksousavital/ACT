/// <reference lib="webworker" />
import { precacheAndRoute, createHandlerBoundToURL } from 'workbox-precaching'
import { registerRoute, NavigationRoute } from 'workbox-routing'
import { NetworkFirst, StaleWhileRevalidate } from 'workbox-strategies'
import { ExpirationPlugin } from 'workbox-expiration'
import { CacheableResponsePlugin } from 'workbox-cacheable-response'
import { clientsClaim } from 'workbox-core'

declare let self: ServiceWorkerGlobalScope

self.skipWaiting()
clientsClaim()

// ── Offline caching — same behavior the previous generateSW `workbox` config provided ────────
precacheAndRoute(self.__WB_MANIFEST)

registerRoute(
  ({ url }) => /\/api\/(client|treatment|treatmenttype|followups)\//i.test(url.pathname),
  new NetworkFirst({
    cacheName: 'api-cache',
    networkTimeoutSeconds: 5,
    plugins: [
      new ExpirationPlugin({ maxEntries: 100, maxAgeSeconds: 60 * 60 }),
      new CacheableResponsePlugin({ statuses: [0, 200] }),
    ],
  })
)

registerRoute(
  ({ url }) => /\/api\/brandsettings/i.test(url.pathname),
  new StaleWhileRevalidate({
    cacheName: 'brand-cache',
    plugins: [
      new ExpirationPlugin({ maxEntries: 10, maxAgeSeconds: 60 * 60 * 24 }),
      new CacheableResponsePlugin({ statuses: [0, 200] }),
    ],
  })
)

registerRoute(
  new NavigationRoute(createHandlerBoundToURL('/index.html'), {
    denylist: [/^\/api/],
  })
)

// ── Web Push ────────────────────────────────────────────────────────────────────────────────
interface PushPayload {
  title?: string
  body?: string
  url?: string
}

function parsePushPayload(event: PushEvent): PushPayload {
  try {
    return event.data?.json() ?? {}
  } catch {
    return { body: event.data?.text() }
  }
}

self.addEventListener('push', (event: PushEvent) => {
  const payload = parsePushPayload(event)
  const title = payload.title ?? 'ACT'
  event.waitUntil(
    self.registration.showNotification(title, {
      body: payload.body,
      icon: '/pwa-192x192.png',
      badge: '/pwa-192x192.png',
      vibrate: [200, 100, 200],
      data: { url: payload.url ?? '/' },
    })
  )
})

self.addEventListener('notificationclick', (event: NotificationEvent) => {
  event.notification.close()
  const targetUrl = (event.notification.data?.url as string) ?? '/'

  event.waitUntil(
    self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
      for (const client of clientList) {
        if (client.url.includes(targetUrl) && 'focus' in client) {
          return client.focus()
        }
      }
      return self.clients.openWindow(targetUrl)
    })
  )
})
