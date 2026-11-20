using ImposterGame.Models;
using ImposterGame.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ImposterGame.Pages
{
    public partial class GameSettingsDialog : ComponentBase
    {
        [Inject] public GameService GameService { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;

        protected GameSettings Settings => GameService.Settings;
        protected IReadOnlyList<WordCategory> Categories => GameService.Categories;
        protected bool CanUseTwoImposters => GameService.Players.Count >= GameService.MinPlayersForTwoImposters;
        protected int EffectiveImposterCount => GameService.GetEffectiveImposterCount();
        protected int ActiveWordCount => GameService.GetActiveWordPoolCount();

        protected string DifficultyDescription => Settings.Difficulty switch
        {
            WordDifficulty.Easy => "فقط کلمات ساده و روزمره — مناسب برای همه",
            WordDifficulty.Medium => "فقط کلمات متعادل و متنوع — چالش استاندارد",
            WordDifficulty.Hard => "فقط کلمات بسیار سخت، تخصصی و مفهومی — برای حرفه‌ای‌ها!",
            _ => string.Empty
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GameService.LoadPlayersAsync();
                await GameService.LoadSettingsAsync();
                StateHasChanged();
            }
        }

        protected async Task SetDifficulty(WordDifficulty difficulty)
        {
            Settings.Difficulty = difficulty;
            await GameService.SaveSettingsAsync();
        }

        protected bool IsCategorySelected(string categoryId) =>
            Settings.SelectedCategoryIds.Contains(categoryId, StringComparer.OrdinalIgnoreCase);

        protected async Task SelectAllCategories()
        {
            Settings.SelectedCategoryIds.Clear();
            await GameService.SaveSettingsAsync();
        }

        protected async Task ToggleCategory(string categoryId)
        {
            var existing = Settings.SelectedCategoryIds
                .FirstOrDefault(id => string.Equals(id, categoryId, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                Settings.SelectedCategoryIds.Remove(existing);
            }
            else
            {
                Settings.SelectedCategoryIds.Add(categoryId);
            }

            if (Settings.SelectedCategoryIds.Count >= Categories.Count)
            {
                Settings.SelectedCategoryIds.Clear();
            }

            await GameService.SaveSettingsAsync();
        }

        protected async Task SetImposterCount(int count)
        {
            if (count > 1 && !CanUseTwoImposters)
            {
                return;
            }

            Settings.ImposterCount = Math.Clamp(count, 1, 2);
            await GameService.SaveSettingsAsync();
        }

        protected async Task OnAutoTimeChanged(bool value)
        {
            Settings.AutoTimePerPlayer = value;
            await GameService.SaveSettingsAsync();
        }

        protected async Task ChangeRoundMinutes(int delta)
        {
            Settings.RoundDurationMinutes = Math.Clamp(
                Settings.RoundDurationMinutes + delta,
                GameService.MinRoundMinutes,
                GameService.MaxRoundMinutes);
            await GameService.SaveSettingsAsync();
        }

        protected async Task OnCategoryHintChanged(bool value)
        {
            Settings.ShowCategoryHintToImposter = value;
            await GameService.SaveSettingsAsync();
        }

        protected void CloseDialog()
        {
            DialogService.Close(true);
        }

        public static DialogOptions CreateDialogOptions() => new()
        {
            Width = "min(94vw, 460px)",
            CloseDialogOnOverlayClick = true,
            ShowClose = true,
            Draggable = false,
            Resizable = false,
            CssClass = "mobile-settings-dialog"
        };
    }
}
