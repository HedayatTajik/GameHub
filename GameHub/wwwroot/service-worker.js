const CACHE_NAME = 'gamehub-pwa-v4';
const OFFLINE_URL = '/offline.html';
const PRECACHE_ASSETS = [
    OFFLINE_URL,
    '/manifest.webmanifest',
    '/Icons/pwa-192.png',
    '/Icons/pwa-512.png',
    '/Icons/pwa-maskable-192.png',
    '/Icons/pwa-maskable-512.png',
    '/Icons/pwa-icon.svg',
    '/Icons/mask.png',
    '/js/gameHub.js'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => cache.addAll(PRECACHE_ASSETS))
            .catch(() => {})
    );
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(keys.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k)))
        )
    );
    self.clients.claim();
});

self.addEventListener('fetch', (event) => {
    const req = event.request;
    if (req.method !== 'GET') return;

    const url = new URL(req.url);
    // Never intercept Blazor SignalR, framework, or cross-origin requests
    if (url.origin !== self.location.origin ||
        url.pathname.startsWith('/_blazor') ||
        url.pathname.startsWith('/_framework') ||
        url.pathname.startsWith('/_content')) {
        return;
    }

    // Navigation requests: network-first with friendly offline.html fallback
    if (req.mode === 'navigate') {
        event.respondWith(
            fetch(req).catch(async () => {
                const cache = await caches.open(CACHE_NAME);
                const offlinePage = await cache.match(OFFLINE_URL);
                return offlinePage || new Response('Offline', { status: 503, statusText: 'Offline' });
            })
        );
        return;
    }

    // Static assets (Icons, JS, manifest): stale-while-revalidate
    if (url.pathname.startsWith('/Icons/') ||
        url.pathname.startsWith('/js/') ||
        url.pathname === '/manifest.webmanifest') {
        event.respondWith(
            caches.open(CACHE_NAME).then(async (cache) => {
                const cached = await cache.match(req);
                const networkPromise = fetch(req).then((resp) => {
                    if (resp && resp.ok) {
                        cache.put(req, resp.clone()).catch(() => {});
                    }
                    return resp;
                }).catch(() => null);

                return cached || (await networkPromise) || new Response('', { status: 504 });
            })
        );
    }
});
