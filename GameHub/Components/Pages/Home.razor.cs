using ImposterGame.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace GameHub.Components.Pages
{
    public partial class Home
    {
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] private GameService GameService { get; set; } = default!;

        protected override void OnInitialized()
        {
            if (GameService.IsGameInProgress)
            {
                GameService.ResetRound();
            }
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
                "⚙️ تنظیمات بازی",
                options: ImposterGame.Pages.GameSettingsDialog.CreateDialogOptions());
        }
    }
}
