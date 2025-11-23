using ImposterGame.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ImposterGame.Services
{
    public class GameService
    {
        private readonly WordService _wordService;
        private readonly Random _random = new();
        private readonly IJSRuntime _jsRuntime;
        private const string LocalStorageKey = "ImposterGamePlayers";
        public List<Player> Players { get; } = new();

        public GameService(WordService wordService, IJSRuntime jsRuntime) 
        {
            _wordService = wordService;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Assigns roles to all players: one imposter, others receive a word.
        /// </summary>
        public void AssignRoles()
        {
            if (!Players.Any()) return;

            var word = _wordService.GetRandomWord();
            int imposterIndex = _random.Next(Players.Count);

            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                player.IsImposter = i == imposterIndex;
                player.Word = player.IsImposter ? null : word;
                player.HasViewedCard = false;
            }
        }

        public async Task LoadPlayersAsync() 
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);
                if (!string.IsNullOrEmpty(json))
                {
                    var storedPlayers = JsonSerializer.Deserialize<List<Player>>(json);
                    if (storedPlayers != null)
                    {
                        Players.Clear();
                        Players.AddRange(storedPlayers);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading players: {ex.Message}");
            }
        }


        private async Task SavePlayersAsync()
        {
            var json = JsonSerializer.Serialize(Players);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
        }

        /// <summary>
        /// Removes a player from the game.
        /// </summary>
        public async Task<List<Player>> DeleteUser(Player player)
        {
            if (player is not null)
            {
                Players.Remove(player);
                await SavePlayersAsync();
            }

            return Players;
        }

        public async Task AddNewPlayerAsync(int id, string name, string uri) 
        {
            var newPlayer = new Player
            {
                Id = id,
                Name = name,
                Uri = uri
            };

            Players.Add(newPlayer);
            await SavePlayersAsync();
        }

        /// <summary>
        /// Returns the current list of players.
        /// </summary>
        public List<Player> ListNewPlayers() => Players;

        /// <summary>
        /// Returns the imposter player if assigned, otherwise null.
        /// </summary>
        public Player? GetImposter() => Players.FirstOrDefault(p => p.IsImposter);
    }
}
