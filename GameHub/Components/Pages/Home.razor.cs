using ImposterGame.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace GameHub.Components.Pages
{
    public partial class Home : IDisposable
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] public GameService GameService { get; set; } = default!;
        [Inject] public LocalizationService L { get; set; } = default!;
        [Inject] private AudioHapticService AudioHaptic { get; set; } = default!;

        protected bool CanInstallPwa { get; private set; }

        protected override void OnInitialized()
        {
            if (GameService.IsGameInProgress)
            {
                GameService.ResetRound();
            }

            GameService.OnSettingsChanged += HandleSettingsChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GameService.LoadSettingsAsync();
                CanInstallPwa = await AudioHaptic.CanInstallPwaAsync();
                StateHasChanged();
            }
        }

        private void HandleSettingsChanged()
        {
            _ = InvokeAsync(StateHasChanged);
        }

        private void GoToGame()
        {
            Navigation.NavigateTo("/impostergame");
        }

        private void GoToAbout()
        {
            Navigation.NavigateTo("/aboutgame");
        }

        private async Task OpenSettings()
        {
            await DialogService.OpenAsync<GameSettingsDialog>(
                L.NavSettingsTitle,
                options: ImposterGame.Pages.GameSettingsDialog.CreateDialogOptions());
        }

        private async Task InstallApp()
        {
            var installed = await AudioHaptic.PromptInstallPwaAsync();
            if (installed)
            {
                CanInstallPwa = false;
            }
        }

        public void Dispose()
        {
            GameService.OnSettingsChanged -= HandleSettingsChanged;
        }
    }
}
