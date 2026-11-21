using Microsoft.JSInterop;

namespace ImposterGame.Services
{
    public class AudioHapticService
    {
        private readonly IJSRuntime _jsRuntime;

        public AudioHapticService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
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

        public async Task<bool> CanInstallPwaAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("gameHub.canInstallPwa");
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PromptInstallPwaAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<bool>("gameHub.promptInstallPwa");
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
    }
}
