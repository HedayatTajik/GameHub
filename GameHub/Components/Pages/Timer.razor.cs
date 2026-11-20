
using ImposterGame.Models;
using ImposterGame.Services;
using Microsoft.AspNetCore.Components;

namespace ImposterGame.Pages
{
    public partial class Timer : ComponentBase, IDisposable
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int PlayerCount { get; set; }

        [Inject] public GameService GameService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private CancellationTokenSource? _timerCts;
        private bool _disposed;

        public List<Player> PlayerList { get; set; } = new();
        public string PersonStart { get; set; } = string.Empty;
        public Player? Imposter { get; private set; }
        public string SecretWord { get; private set; } = string.Empty;

        public int timeLeft = 180;
        public bool isRunning = false;
        public bool isPaused = false;
        public bool showResult = false;

        public bool IsTimeUp => timeLeft <= 0;
        public string formattedTime => $"{Math.Max(0, timeLeft) / 60:D2}:{Math.Max(0, timeLeft) % 60:D2}";

        protected override void OnInitialized()
        {
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

        private void InitializeStateFromService()
        {
            PlayerList = GameService.ListNewPlayers();
            Imposter = GameService.GetImposter();
            SecretWord = GameService.CurrentWord;

            int effectiveCount = PlayerCount > 0
                ? PlayerCount
                : (PlayerList.Count > 0 ? PlayerList.Count : GameService.MinPlayers);

            timeLeft = 60 * Math.Max(1, effectiveCount);

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

        public void TogglePause()
        {
            if (!IsTimeUp)
            {
                isPaused = !isPaused;
            }
        }

        public void RevealResult()
        {
            showResult = true;
            isPaused = true;
        }

        public void StartAgain()
        {
            _timerCts?.Cancel();
            GameService.ResetRound();
            NavigationManager.NavigateTo("/impostergame");
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _timerCts?.Cancel();
            _timerCts?.Dispose();
        }
    }
}