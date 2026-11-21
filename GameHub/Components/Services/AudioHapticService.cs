using Microsoft.JSInterop;

namespace ImposterGame.Services
{
    public class AudioHapticService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<AudioHapticService>? _dotNetRef;
        private bool _isSubscribed;

        public bool CanInstallPwa { get; private set; }
        public bool IsStandalone { get; private set; }
        public bool IsIosInstallHintEligible { get; private set; }

        public event Action? OnPwaStateChanged;

        public AudioHapticService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task EnsurePwaSubscribedAsync()
        {
            if (_isSubscribed)
            {
                return;
            }

            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                IsStandalone = await _jsRuntime.InvokeAsync<bool>("gameHub.isStandalone");
                IsIosInstallHintEligible = await _jsRuntime.InvokeAsync<bool>("gameHub.isIosInstallHintEligible");
                CanInstallPwa = await _jsRuntime.InvokeAsync<bool>("gameHub.canInstallPwa");
                await _jsRuntime.InvokeVoidAsync("gameHub.subscribePwaInstall", _dotNetRef);
                _isSubscribed = true;
                OnPwaStateChanged?.Invoke();
            }
            catch (InvalidOperationException)
            {
                // Prerendering
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PWA subscription error: {ex.Message}");
            }
        }

        [JSInvokable]
        public void NotifyPwaInstallState(bool canInstall, bool isStandalone)
        {
            var changed = CanInstallPwa != canInstall || IsStandalone != isStandalone;
            CanInstallPwa = canInstall;
            IsStandalone = isStandalone;
            if (isStandalone)
            {
                IsIosInstallHintEligible = false;
            }

            if (changed)
            {
                OnPwaStateChanged?.Invoke();
            }
        }

        public async Task ApplyPreferencesAsync(bool darkMode, string language, string direction)
        {
            await SafeInvokeAsync("gameHub.applyPreferences", darkMode, language, direction);
        }

        public async Task PlayCardRevealAsync(bool enabled)
        {
            if (enabled)
            {
                await SafeInvokeAsync("gameHub.playCardReveal");
            }
        }

        public async Task PlayCardHideAsync(bool enabled)
        {
            if (enabled)
            {
                await SafeInvokeAsync("gameHub.playCardHide");
            }
        }

        public async Task PlayTickAsync(bool enabled, int secondsLeft)
        {
            if (enabled)
            {
                await SafeInvokeAsync("gameHub.playTick", secondsLeft);
            }
        }

        public async Task PlayTimeUpAsync(bool enabled)
        {
            if (enabled)
            {
                await SafeInvokeAsync("gameHub.playTimeUp");
            }
        }

        public async Task PlayRevealResultAsync(bool enabled)
        {
            if (enabled)
            {
                await SafeInvokeAsync("gameHub.playRevealResult");
            }
        }

        public async Task PlayClickAsync(bool enabled)
        {
            if (enabled)
            {
                await SafeInvokeAsync("gameHub.playClick");
            }
        }

        public async Task<bool> PromptInstallPwaAsync()
        {
            try
            {
                var accepted = await _jsRuntime.InvokeAsync<bool>("gameHub.promptInstallPwa");
                if (accepted)
                {
                    CanInstallPwa = false;
                    OnPwaStateChanged?.Invoke();
                }

                return accepted;
            }
            catch
            {
                return false;
            }
        }

        private async Task SafeInvokeAsync(string identifier, params object?[] args)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync(identifier, args);
            }
            catch (InvalidOperationException)
            {
                // Prerendering: JS interop not available yet.
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JS interop error ({identifier}): {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_isSubscribed && _dotNetRef is not null)
            {
                await SafeInvokeAsync("gameHub.unsubscribePwaInstall", _dotNetRef);
            }

            _dotNetRef?.Dispose();
            _dotNetRef = null;
        }
    }
}
