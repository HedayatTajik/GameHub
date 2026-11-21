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
        [Inject] public LocalizationService L { get; set; } = default!;
        [Inject] private AudioHapticService AudioHaptic { get; set; } = default!;

        public int currentIndex = 0;

        public bool gameStarted = false;
        public bool isHoldingCard = false;
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

        public string SettingsSummary =>
            L.BuildSettingsSummary(Settings, EffectiveImposterCount, GameService.GetActiveWordPoolCount());

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
                L.NavSettingsTitle,
                options: GameSettingsDialog.CreateDialogOptions());
        }

        public async Task AddPlayer()
        {
            validationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(newPlayer.Name))
            {
                validationMessage = L.ValidationNameRequired;
                return;
            }

            if (GameService.PlayerNameExists(newPlayer.Name))
            {
                validationMessage = L.ValidationDuplicateName;
                return;
            }

            if (string.IsNullOrEmpty(selectedAvatar))
            {
                validationMessage = L.ValidationAvatarRequired;
                return;
            }

            await GameService.AddNewPlayerAsync(
                name: newPlayer.Name,
                uri: selectedAvatar
            );

            await AudioHaptic.PlayClickAsync(Settings.SoundEnabled);

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
            await AudioHaptic.PlayClickAsync(Settings.SoundEnabled);
        }

        public async Task AssignRoles()
        {
            if (players.Count < GameService.MinPlayers)
            {
                validationMessage = L.ValidationMinPlayers(GameService.MinPlayers);
                return;
            }

            validationMessage = string.Empty;
            _isTransitioningToTimer = false;
            isHoldingCard = false;
            currentIndex = 0;
            GameService.AssignRoles();
            gameStarted = true;
            await AudioHaptic.PlayClickAsync(Settings.SoundEnabled);
        }

        public async Task CancelGame()
        {
            isHoldingCard = false;
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

            isHoldingCard = false;
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
                    L.ConfirmExitMessage,
                    L.ConfirmExitTitle,
                    new ConfirmOptions
                    {
                        OkButtonText = L.ConfirmExitOk,
                        CancelButtonText = L.ConfirmContinue,
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

        public async Task StartHoldCard()
        {
            if (isHoldingCard || CurrentPlayer is null)
            {
                return;
            }

            isHoldingCard = true;
            CurrentPlayer.HasViewedCard = true;
            await AudioHaptic.PlayCardRevealAsync(Settings.SoundEnabled);
        }

        public async Task EndHoldCard()
        {
            if (!isHoldingCard)
            {
                return;
            }

            isHoldingCard = false;
            await AudioHaptic.PlayCardHideAsync(Settings.SoundEnabled);
        }

        public async Task HandleHoldKeyDown(KeyboardEventArgs e)
        {
            if (e.Key is " " or "Enter")
            {
                await StartHoldCard();
            }
        }

        public async Task HandleHoldKeyUp(KeyboardEventArgs e)
        {
            if (e.Key is " " or "Enter")
            {
                await EndHoldCard();
            }
        }

        public async Task NextPlayer()
        {
            isHoldingCard = false;
            if (currentIndex < GameService.Players.Count - 1)
            {
                currentIndex++;
                await AudioHaptic.PlayClickAsync(Settings.SoundEnabled);
            }
        }

        public async Task StartGame()
        {
            isHoldingCard = false;
            _isTransitioningToTimer = true;
            await AudioHaptic.PlayClickAsync(Settings.SoundEnabled);
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
                : CurrentPlayer.IsImposter ? L.ImposterCardTitle : CurrentPlayer.Word;

        public void Dispose()
        {
            GameService.OnSettingsChanged -= HandleSettingsChanged;
        }
    }
}
