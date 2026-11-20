using ImposterGame.Models;
using ImposterGame.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ImposterGame.Pages
{
    public partial class ImposterGamePage : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] public GameService GameService { get; set; } = default!;

        public int currentIndex = 0;

        public bool gameStarted = false;
        public string selectedAvatar = string.Empty;
        public string validationMessage = string.Empty;

        public List<Player> players = new();
        public Player newPlayer = new();

        public Player? CurrentPlayer =>
            currentIndex >= 0 && currentIndex < GameService.Players.Count
                ? GameService.Players[currentIndex]
                : null;

        protected override void OnInitialized()
        {
            players = GameService.Players;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GameService.LoadPlayersAsync();
                players = GameService.ListNewPlayers();
                StateHasChanged();
            }
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
            currentIndex = 0;
            GameService.AssignRoles();
            gameStarted = true;
        }

        public void CancelGame()
        {
            GameService.ResetRound();
            currentIndex = 0;
            gameStarted = false;
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
                : CurrentPlayer.IsImposter ? "شما شیاد هستین" : CurrentPlayer.Word;
    }
}
