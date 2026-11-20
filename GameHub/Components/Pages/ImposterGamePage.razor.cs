using ImposterGame.Models;
using ImposterGame.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace ImposterGame.Pages
{
    public partial class ImposterGamePage : ComponentBase, IDisposable
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] public GameService GameService { get; set; } = default!;

        public int currentIndex = 0;

        public bool gameStarted = false;
        public string selectedAvatar = string.Empty;
        public string validationMessage = string.Empty;

        private bool _isTransitioningToTimer = false;
        private bool _isConfirmDialogOpen = false;

        protected bool ShouldConfirmExit => gameStarted && !_isTransitioningToTimer;

        public List<Player> players = new();
        public Player newPlayer = new();

        public Player? CurrentPlayer =>
            currentIndex >= 0 && currentIndex < GameService.Players.Count
                ? GameService.Players[currentIndex]
                : null;

        public GameSettings Settings => GameService.Settings;
        public int EffectiveImposterCount => GameService.GetEffectiveImposterCount();

        public string SettingsSummary
        {
            get
            {
                var levelPart = GameService.GetDifficultyLabel(Settings.Difficulty);

                var categoryPart = Settings.SelectedCategoryIds.Count == 0
                    ? "همه دسته‌ها"
                    : $"{Settings.SelectedCategoryIds.Count} دسته";

                var imposterPart = $"{EffectiveImposterCount} شیاد";

                var timePart = Settings.AutoTimePerPlayer
                    ? "زمان خودکار"
                    : $"{Settings.RoundDurationMinutes} دقیقه";

                var hintPart = Settings.ShowCategoryHintToImposter ? " • با راهنمایی" : string.Empty;

                return $"{levelPart} • {categoryPart} ({GameService.GetActiveWordPoolCount()} کلمه) • {imposterPart} • {timePart}{hintPart}";
            }
        }

        protected override void OnInitialized()
        {
            if (GameService.IsGameInProgress)
            {
                GameService.ResetRound();
            }

            players = GameService.Players;
            GameService.OnSettingsChanged += HandleSettingsChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GameService.LoadPlayersAsync();
                await GameService.LoadSettingsAsync();
                players = GameService.ListNewPlayers();
                StateHasChanged();
            }
        }

        private void HandleSettingsChanged()
        {
            _ = InvokeAsync(StateHasChanged);
        }

        public async Task OpenSettingsDialog()
        {
            await DialogService.OpenAsync<GameHub.Components.Pages.GameSettingsDialog>(
                "⚙️ تنظیمات بازی",
                options: GameSettingsDialog.CreateDialogOptions());
        }

        public async Task AddPlayer()
        {
            validationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(newPlayer.Name))
            {
                validationMessage = "لطفاً نام بازیکن را وارد کنید.";
                return;
            }

            if (GameService.PlayerNameExists(newPlayer.Name))
            {
                validationMessage = "این نام قبلاً اضافه شده است.";
                return;
            }

            if (string.IsNullOrEmpty(selectedAvatar))
            {
                validationMessage = "لطفاً یک آواتار انتخاب کنید.";
                return;
            }

            await GameService.AddNewPlayerAsync(
                name: newPlayer.Name,
                uri: selectedAvatar
            );

            players = GameService.ListNewPlayers();
            selectedAvatar = string.Empty;
            newPlayer = new Player();
        }

        public void OnNameChanged(string value)
        {
            newPlayer.Name = value ?? string.Empty;
            if (!string.IsNullOrEmpty(validationMessage))
            {
                validationMessage = string.Empty;
            }
        }

        public async Task HandleKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await AddPlayer();
            }
        }

        public async Task RemovePlayer(Player player)
        {
            players = await GameService.DeleteUser(player);
            validationMessage = string.Empty;
        }

        public void AssignRoles()
        {
            if (players.Count < GameService.MinPlayers)
            {
                validationMessage = $"برای شروع بازی حداقل {GameService.MinPlayers} بازیکن لازم است.";
                return;
            }

            validationMessage = string.Empty;
            _isTransitioningToTimer = false;
            currentIndex = 0;
            GameService.AssignRoles();
            gameStarted = true;
        }

        public async Task CancelGame()
        {
            if (await ConfirmExitGameAsync())
            {
                GameService.ResetRound();
                currentIndex = 0;
                gameStarted = false;
            }
        }

        protected async Task HandleBeforeInternalNavigation(LocationChangingContext context)
        {
            if (!ShouldConfirmExit)
            {
                return;
            }

            if (_isConfirmDialogOpen)
            {
                context.PreventNavigation();
                return;
            }

            var confirmed = await ConfirmExitGameAsync();
            if (!confirmed)
            {
                context.PreventNavigation();
                return;
            }

            gameStarted = false;
            GameService.ResetRound();
        }

        private async Task<bool> ConfirmExitGameAsync()
        {
            if (_isConfirmDialogOpen)
            {
                return false;
            }

            _isConfirmDialogOpen = true;
            try
            {
                var result = await DialogService.Confirm(
                    "بازی در حال اجراست. آیا مطمئن هستید که می‌خواهید از بازی خارج شوید؟",
                    "⚠️ توقف بازی",
                    new ConfirmOptions
                    {
                        OkButtonText = "بله، خروج از بازی",
                        CancelButtonText = "ادامه بازی",
                        CloseDialogOnOverlayClick = true,
                        ShowClose = false,
                        Draggable = false,
                        Width = "min(92vw, 380px)",
                        CssClass = "mobile-confirm-dialog"
                    });

                return result == true;
            }
            finally
            {
                _isConfirmDialogOpen = false;
            }
        }

        public void ShowCard()
        {
            if (CurrentPlayer is not null)
            {
                CurrentPlayer.HasViewedCard = true;
            }
        }

        public void NextPlayer()
        {
            if (currentIndex < GameService.Players.Count - 1)
            {
                currentIndex++;
            }
        }

        public void StartGame()
        {
            _isTransitioningToTimer = true;
            NavigationManager.NavigateTo($"/timer?playerCount={players.Count}");
        }

        public void HandleAvatarSelected(string avatar)
        {
            selectedAvatar = avatar;
            if (!string.IsNullOrEmpty(validationMessage))
            {
                validationMessage = string.Empty;
            }
        }

        public string CardText =>
            CurrentPlayer is null
                ? string.Empty
                : CurrentPlayer.IsImposter ? "🕵️ شما شیاد هستید!" : CurrentPlayer.Word;

        public void Dispose()
        {
            GameService.OnSettingsChanged -= HandleSettingsChanged;
        }
    }
}
