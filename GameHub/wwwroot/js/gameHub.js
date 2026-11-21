(function () {
    let audioCtx = null;
    let deferredInstallPrompt = null;
    const pwaSubscribers = new Set();

    function getAudioContext() {
        const AudioContextClass = window.AudioContext || window.webkitAudioContext;
        if (!AudioContextClass) return null;
        if (!audioCtx) {
            audioCtx = new AudioContextClass();
        }
        if (audioCtx.state === 'suspended') {
            audioCtx.resume().catch(() => {});
        }
        return audioCtx;
    }

    function playTone(freq, duration, type, volume, delay) {
        const ctx = getAudioContext();
        if (!ctx) return;

        const now = ctx.currentTime + (delay || 0);
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = type || 'sine';
        osc.frequency.setValueAtTime(freq, now);

        gain.gain.setValueAtTime(0.0001, now);
        gain.gain.exponentialRampToValueAtTime(volume || 0.14, now + 0.015);
        gain.gain.exponentialRampToValueAtTime(0.0001, now + duration);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(now);
        osc.stop(now + duration + 0.02);
    }

    function vibrate(pattern) {
        try {
            if (navigator && typeof navigator.vibrate === 'function') {
                navigator.vibrate(pattern);
            }
        } catch {
            // Vibration not supported or blocked
        }
    }

    function isStandaloneMode() {
        return window.matchMedia('(display-mode: standalone)').matches ||
            window.navigator.standalone === true;
    }

    function notifySubscribers() {
        const canInstall = deferredInstallPrompt !== null && !isStandaloneMode();
        const standalone = isStandaloneMode();
        pwaSubscribers.forEach((dotNetRef) => {
            dotNetRef.invokeMethodAsync('NotifyPwaInstallState', canInstall, standalone).catch(() => {
                pwaSubscribers.delete(dotNetRef);
            });
        });
    }

    // Unlock audio context on first user gesture for mobile Safari/Chrome
    ['pointerdown', 'touchstart', 'keydown'].forEach(evt => {
        window.addEventListener(evt, () => {
            if (audioCtx && audioCtx.state === 'suspended') {
                audioCtx.resume().catch(() => {});
            }
        }, { passive: true });
    });

    // Capture PWA install prompt and notify Blazor immediately
    window.addEventListener('beforeinstallprompt', (e) => {
        e.preventDefault();
        deferredInstallPrompt = e;
        notifySubscribers();
    });

    window.addEventListener('appinstalled', () => {
        deferredInstallPrompt = null;
        notifySubscribers();
    });

    // Register lightweight PWA Service Worker reliably
    function registerServiceWorker() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/service-worker.js', { scope: '/' }).catch(() => {});
        }
    }

    if (document.readyState === 'complete') {
        registerServiceWorker();
    } else {
        window.addEventListener('load', registerServiceWorker);
    }

    window.gameHub = {
        applyPreferences: function (darkMode, language, direction) {
            const root = document.documentElement;
            root.setAttribute('data-theme', darkMode ? 'dark' : 'light');
            root.setAttribute('lang', language || 'fa');
            root.setAttribute('dir', direction || 'rtl');

            const metaTheme = document.querySelector('meta[name="theme-color"]');
            if (metaTheme) {
                metaTheme.setAttribute('content', darkMode ? '#13111C' : '#FAF7F2');
            }
        },

        playClick: function () {
            playTone(540, 0.055, 'sine', 0.08, 0);
            vibrate(10);
        },

        playCardReveal: function () {
            playTone(523.25, 0.12, 'triangle', 0.12, 0);
            playTone(659.25, 0.16, 'sine', 0.12, 0.06);
            vibrate(18);
        },

        playCardHide: function () {
            playTone(392.0, 0.07, 'sine', 0.07, 0);
        },

        playTick: function (secondsLeft) {
            const urgent = secondsLeft <= 3;
            const freq = urgent ? 960 : 720;
            playTone(freq, urgent ? 0.09 : 0.06, 'triangle', urgent ? 0.16 : 0.1, 0);
            vibrate(urgent ? 35 : 15);
        },

        playTimeUp: function () {
            playTone(880, 0.18, 'triangle', 0.2, 0);
            playTone(880, 0.18, 'triangle', 0.2, 0.22);
            playTone(1174.66, 0.42, 'sine', 0.22, 0.44);
            vibrate([180, 80, 180, 80, 320]);
        },

        playRevealResult: function () {
            playTone(392.0, 0.12, 'triangle', 0.13, 0);
            playTone(493.88, 0.12, 'triangle', 0.14, 0.1);
            playTone(587.33, 0.14, 'triangle', 0.15, 0.2);
            playTone(783.99, 0.36, 'sine', 0.18, 0.32);
            vibrate([30, 30, 110]);
        },

        subscribePwaInstall: function (dotNetRef) {
            pwaSubscribers.add(dotNetRef);
            notifySubscribers();
        },

        unsubscribePwaInstall: function (dotNetRef) {
            pwaSubscribers.delete(dotNetRef);
        },

        canInstallPwa: function () {
            return deferredInstallPrompt !== null && !isStandaloneMode();
        },

        isStandalone: function () {
            return isStandaloneMode();
        },

        isIosInstallHintEligible: function () {
            const ua = window.navigator.userAgent || '';
            const isIos = /iphone|ipad|ipod/i.test(ua) ||
                (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
            return isIos && !isStandaloneMode();
        },

        promptInstallPwa: async function () {
            if (!deferredInstallPrompt) return false;
            deferredInstallPrompt.prompt();
            try {
                const choice = await deferredInstallPrompt.userChoice;
                deferredInstallPrompt = null;
                notifySubscribers();
                return choice && choice.outcome === 'accepted';
            } catch {
                deferredInstallPrompt = null;
                notifySubscribers();
                return false;
            }
        }
    };
})();
