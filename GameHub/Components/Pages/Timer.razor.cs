using ImposterGame.Models;
using ImposterGame.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Radzen;
using System.Globalization;

namespace ImposterGame.Pages
{
    public partial class Timer : ComponentBase, IDisposable
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int PlayerCount { get; set; }

        [Inject] public GameService GameService { get; set; } = default!;
        [Inject] public LocalizationService L { get; set; } = default!;
        [Inject] private AudioHapticService AudioHaptic { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;

        private CancellationTokenSource? _timerCts;
        private bool _disposed;
        private bool _isExitConfirmed;
        private bool _isConfirmDialogOpen;

        public List<Player> PlayerList { get; set; } = new();
        public string PersonStart { get; set; } = string.Empty;
        public List<Player> Imposters { get; private set; } = new();
        public string SecretWord { get; private set; } = string.Empty;
        public string SecretCategoryName { get; private set; } = string.Empty;
        public string SecretCategoryIcon { get; private set; } = string.Empty;
        public WordDifficulty SecretDifficulty { get; private set; } = WordDifficulty.Medium;

        public int timeLeft = 180;
        public int totalDurationSeconds = 180;
        public bool isRunning = false;
        public bool isPaused = false;
        public bool showResult = false;

        private const double RingRadius = 92;
        public static readonly double RingCircumference = 2 * Math.PI * RingRadius;

        public bool IsTimeUp => timeLeft <= 0;
        protected bool ShouldConfirmExit => GameService.IsGameInProgress && !showResult && !_isExitConfirmed;
        public string formattedTime => $"{Math.Max(0, timeLeft) / 60:D2}:{Math.Max(0, timeLeft) % 60:D2}";

        public string RingCircumferenceCss => RingCircumference.ToString("F2", CultureInfo.InvariantCulture);

        public string RingDashOffsetCss
        {
            get
            {
                var ratio = totalDurationSeconds > 0
                    ? Math.Clamp((double)timeLeft / totalDurationSeconds, 0.0, 1.0)
                    : 0.0;
                var offset = RingCircumference * (1.0 - ratio);
                return offset.ToString("F2", CultureInfo.InvariantCulture);
            }
        }

        public string TimerRingStateClass =>
            timeLeft <= 10 ? "danger" :
            timeLeft <= 30 ? "warning" :
            "calm";

        public string TimerStatusLabel =>
            IsTimeUp ? L.TimerStatusDone :
            isPaused ? L.TimerStatusPaused :
            L.TimerStatusRunning;

        protected override void OnInitialized()
        {
            GameService.OnSettingsChanged += HandleSettingsChanged;
            InitializeStateFromService();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            if (GameService.Players.Count == 0)
            {
                await GameService.LoadPlayersAsync();
                await GameService.LoadSettingsAsync();
                InitializeStateFromService();
            }

            if (PlayerList.Count == 0)
            {
                NavigationManager.NavigateTo("/impostergame");
                return;
            }

            StateHasChanged();
            StartCountdown();
        }

        private void HandleSettingsChanged()
        {
            _ = InvokeAsync(StateHasChanged);
        }

        private void InitializeStateFromService()
        {
            PlayerList = GameService.ListNewPlayers();
            Imposters = GameService.GetImposters();
            SecretWord = GameService.CurrentWord;
            SecretCategoryName = GameService.CurrentCategoryName;
            SecretCategoryIcon = GameService.CurrentCategoryIcon;
            SecretDifficulty = GameService.CurrentDifficulty;

            if (GameService.HasActiveGame)
            {
                timeLeft = GameService.GetRoundDurationSeconds();
            }
            else if (PlayerCount > 0)
            {
                timeLeft = 60 * PlayerCount;
            }
            else
            {
                timeLeft = GameService.GetRoundDurationSeconds();
            }

            totalDurationSeconds = Math.Max(1, timeLeft);

            if (PlayerList.Count > 0 && string.IsNullOrEmpty(PersonStart))
            {
                int randomIndex = Random.Shared.Next(PlayerList.Count);
                PersonStart = PlayerList[randomIndex].Name;
            }
        }

        private void StartCountdown()
        {
            if (isRunning || _disposed)
            {
                return;
            }

            _timerCts?.Cancel();
            _timerCts?.Dispose();
            _timerCts = new CancellationTokenSource();

            _ = RunTimerAsync(_timerCts.Token);
        }

        private async Task RunTimerAsync(CancellationToken cancellationToken)
        {
            isRunning = true;

            try
            {
                using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

                while (timeLeft > 0 && await periodicTimer.WaitForNextTickAsync(cancellationToken))
                {
                    if (!isPaused)
                    {
                        timeLeft--;
                        await InvokeAsync(StateHasChanged);

                        if (timeLeft == 0)
                        {
                            await AudioHaptic.PlayTimeUpAsync(GameService.Settings.SoundEnabled);
                        }
                        else if (timeLeft <= 10)
                        {
                            await AudioHaptic.PlayTickAsync(GameService.Settings.SoundEnabled, timeLeft);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when navigating away or disposing component.
            }
            catch (ObjectDisposedException)
            {
                // Component or circuit was disposed during shutdown.
            }
            finally
            {
                isRunning = false;
            }
        }

        public async Task TogglePause()
        {
            if (!IsTimeUp)
            {
                isPaused = !isPaused;
                await AudioHaptic.PlayClickAsync(GameService.Settings.SoundEnabled);
            }
        }

        public async Task RevealResult()
        {
            if (!IsTimeUp)
            {
                var confirmed = await ConfirmActionAsync(
                    L.ConfirmRevealMessage,
                    L.ConfirmRevealTitle,
                    L.ConfirmRevealOk,
                    L.ConfirmContinue);

                if (!confirmed)
                {
                    return;
                }
            }

            showResult = true;
            isPaused = true;
            await AudioHaptic.PlayRevealResultAsync(GameService.Settings.SoundEnabled);
        }

        public async Task StartAgain()
        {
            if (ShouldConfirmExit)
            {
                var confirmed = await ConfirmActionAsync(
                    L.ConfirmRestartMessage,
                    L.ConfirmRestartTitle,
                    L.ConfirmRestartOk,
                    L.ConfirmContinue);

                if (!confirmed)
                {
                    return;
                }
            }

            _isExitConfirmed = true;
            _timerCts?.Cancel();
            GameService.ResetRound();
            await AudioHaptic.PlayClickAsync(GameService.Settings.SoundEnabled);
            NavigationManager.NavigateTo("/impostergame");
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

            var confirmed = await ConfirmActionAsync(
                L.ConfirmExitMessage,
                L.ConfirmExitTitle,
                L.ConfirmExitOk,
                L.ConfirmContinue);

            if (!confirmed)
            {
                context.PreventNavigation();
                return;
            }

            _isExitConfirmed = true;
            _timerCts?.Cancel();
            GameService.ResetRound();
        }

        private async Task<bool> ConfirmActionAsync(string message, string title, string okText, string cancelText)
        {
            if (_isConfirmDialogOpen)
            {
                return false;
            }

            _isConfirmDialogOpen = true;
            var wasPaused = isPaused;
            isPaused = true;

            try
            {
                var result = await DialogService.Confirm(
                    message,
                    title,
                    new ConfirmOptions
                    {
                        OkButtonText = okText,
                        CancelButtonText = cancelText,
                        CloseDialogOnOverlayClick = true,
                        ShowClose = false,
                        Draggable = false,
                        Width = "min(92vw, 380px)",
                        CssClass = "mobile-confirm-dialog"
                    });

                if (result == true)
                {
                    return true;
                }

                isPaused = wasPaused;
                return false;
            }
            finally
            {
                _isConfirmDialogOpen = false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            GameService.OnSettingsChanged -= HandleSettingsChanged;
            _timerCts?.Cancel();
            _timerCts?.Dispose();
        }
    }
}
