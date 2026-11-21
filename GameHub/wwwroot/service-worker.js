const CACHE_NAME = 'gamehub-static-v3';
const PRECACHE_ASSETS = [
    '/manifest.webmanifest',
    '/Icons/pwa-192.png',
    '/Icons/pwa-512.png',
    '/Icons/pwa-icon.svg',
    '/Icons/mask.png'
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
    // Never intercept Blazor SignalR or framework requests
    if (url.pathname.startsWith('/_blazor') || url.pathname.startsWith('/_framework')) {
        return;
    }

    // Cache-first only for static icons/images; network-first for everything else
    if (url.pathname.startsWith('/Icons/')) {
        event.respondWith(
            caches.match(req).then((cached) =>
                cached || fetch(req).then((resp) => {
                    if (resp && resp.ok) {
                        const copy = resp.clone();
                        caches.open(CACHE_NAME).then((cache) => cache.put(req, copy)).catch(() => {});
                    }
                    return resp;
                })
            )
        );
    }
});
