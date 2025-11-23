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

        protected override async Task OnInitializedAsync()
        {
            // Load existing players from GameService (state is preserved!)
            players = GameService.Players;
            await GameService.LoadPlayersAsync();
        }

        public async Task AddPlayer() // 👈 Make this method async
        {
            if (!string.IsNullOrWhiteSpace(newPlayer.Name) &&
                !string.IsNullOrEmpty(selectedAvatar))
            {
                // 1. Call the service method to create and add the player
                await GameService.AddNewPlayerAsync(
                    id: nextPlayerId,
                    name: newPlayer.Name,
                    uri: selectedAvatar
                );

                // 2. Update the component state variables
                nextPlayerId++; // Increment ID for the next player

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
