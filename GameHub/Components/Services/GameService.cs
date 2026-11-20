using ImposterGame.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ImposterGame.Services
{
    public class GameService
    {
        public const int MinPlayers = 3;

        private readonly WordService _wordService;
        private readonly IJSRuntime _jsRuntime;
        private const string LocalStorageKey = "ImposterGamePlayers";
        private bool _hasLoadedFromStorage;

        public List<Player> Players { get; } = new();
        public string CurrentWord { get; private set; } = string.Empty;
        public bool HasActiveGame => Players.Count >= MinPlayers && Players.Any(p => p.IsImposter);

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
            if (Players.Count < MinPlayers) return;

            var word = _wordService.GetRandomWord();
            int imposterIndex = Random.Shared.Next(Players.Count);
            CurrentWord = word;

            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                player.IsImposter = i == imposterIndex;
                player.Word = player.IsImposter ? string.Empty : word;
                player.HasViewedCard = false;
            }
        }

        /// <summary>
        /// Clears role assignments and round state while keeping the player roster.
        /// </summary>
        public void ResetRound()
        {
            CurrentWord = string.Empty;
            foreach (var player in Players)
            {
                player.IsImposter = false;
                player.Word = string.Empty;
                player.HasViewedCard = false;
            }
        }

        public async Task LoadPlayersAsync()
        {
            if (_hasLoadedFromStorage && Players.Count > 0)
            {
                return;
            }

            try
            {
                var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LocalStorageKey);
                if (!string.IsNullOrEmpty(json))
                {
                    var storedPlayers = JsonSerializer.Deserialize<List<Player>>(json);
                    if (storedPlayers != null)
                    {
                        var validPlayers = storedPlayers
                            .Where(p => p is not null && !string.IsNullOrWhiteSpace(p.Name))
                            .ToList();

                        Players.Clear();
                        Players.AddRange(validPlayers);
                    }
                }

                _hasLoadedFromStorage = true;
            }
            catch (InvalidOperationException)
            {
                // Prerendering: JS interop is not available yet.
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected while reading storage.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading players: {ex.Message}");
            }
        }

        private async Task SavePlayersAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(Players);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
            }
            catch (InvalidOperationException)
            {
                // Prerendering: JS interop is not available yet.
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected while saving storage.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving players: {ex.Message}");
            }
        }

        public bool PlayerNameExists(string name)
        {
            var trimmed = name.Trim();
            return Players.Any(p => string.Equals(p.Name.Trim(), trimmed, StringComparison.OrdinalIgnoreCase));
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

        public async Task AddNewPlayerAsync(string name, string uri)
        {
            var nextId = Players.Count > 0 ? Players.Max(p => p.Id) + 1 : 1;
            var newPlayer = new Player
            {
                Id = nextId,
                Name = name.Trim(),
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
