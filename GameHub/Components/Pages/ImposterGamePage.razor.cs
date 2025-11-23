using ImposterGame.Models;
using ImposterGame.Services;
using Microsoft.AspNetCore.Components;

namespace ImposterGame.Pages
{
    public partial class ImposterGamePage : ComponentBase
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private WordService WordService { get; set; } = default!;
        [Inject] public GameService GameService { get; set; } = default!;

        public int currentIndex = 0;
        public int nextPlayerId = 1;

        public bool gameStarted = false;
        public string selectedAvatar = string.Empty;

        public List<Player> players = new();
        public Player newPlayer = new();

        public Player CurrentPlayer => GameService.Players[currentIndex];

        protected override void OnInitialized()
        {
            // Load existing players from GameService (state is preserved!)
            players = GameService.Players;
        }

        public void AddPlayer()
        {
            if (!string.IsNullOrWhiteSpace(newPlayer.Name) &&
                !string.IsNullOrEmpty(selectedAvatar))
            {
                var player = new Player
                {
                    Id = nextPlayerId++,
                    Name = newPlayer.Name,
                    Uri = selectedAvatar
                };

                GameService.Players.Add(player);

                players = GameService.ListNewPlayers();

                selectedAvatar = string.Empty;
                newPlayer = new Player();
            }
        }

        public async Task RemovePlayer(Player player)
        {
            players = await GameService.DeleteUser(player);
        }

        public void AssignRoles()
        {
            GameService.AssignRoles();
            gameStarted = true;
        }

        public void ShowCard()
        {
            CurrentPlayer.HasViewedCard = true;
        }

        public void NextPlayer()
        {
            if (currentIndex < GameService.Players.Count - 1)
                currentIndex++;
        }

        public void StartGame()
        {
            NavigationManager.NavigateTo($"/timer?playerCount={players.Count}");
        }

        public void HandleAvatarSelected(string avatar)
        {
            selectedAvatar = avatar;
        }

        public string CardText =>
            CurrentPlayer.IsImposter ? "شما شیاد هستین" : CurrentPlayer.Word;
    }
}
