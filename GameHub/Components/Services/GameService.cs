using ImposterGame.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ImposterGame.Services
{
    public class GameService
    {
        public const int MinPlayers = 3;
        public const int MinPlayersForTwoImposters = 5;
        public const int MinRoundMinutes = 1;
        public const int MaxRoundMinutes = 15;
        public const int AvatarCount = 26;

        public static string GetAvatarUri(int index)
        {
            var normalized = ((Math.Max(1, index) - 1) % AvatarCount) + 1;
            return $"/Icons/avatar_{normalized}.svg";
        }

        private static bool IsValidAvatarUri(string? uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return false;
            }

            for (int i = 1; i <= AvatarCount; i++)
            {
                if (string.Equals(uri, GetAvatarUri(i), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private readonly WordService _wordService;
        private readonly IJSRuntime _jsRuntime;
        private readonly LocalizationService _localization;
        private readonly AudioHapticService _audioHaptic;
        private const string LocalStorageKey = "ImposterGamePlayers";
        private const string SettingsStorageKey = "ImposterGameSettings";

        private bool _hasLoadedFromStorage;
        private bool _hasLoadedSettings;

        public List<Player> Players { get; } = new();
        public GameSettings Settings { get; private set; } = new();
        public IReadOnlyList<WordCategory> Categories => _wordService.GetCategories(Settings.Language);
        public int TotalWordCount => _wordService.GetTotalWordCount(Settings.Language);

        public event Action? OnSettingsChanged;
        public event Action? OnGameStateChanged;

        public string CurrentWord { get; private set; } = string.Empty;
        public string CurrentCategoryName { get; private set; } = string.Empty;
        public string CurrentCategoryIcon { get; private set; } = string.Empty;
        public WordDifficulty CurrentDifficulty { get; private set; } = WordDifficulty.Medium;
        public int ActiveImposterCount { get; private set; }
        public bool IsGameInProgress { get; private set; }

        public bool HasActiveGame => Players.Count >= MinPlayers && Players.Any(p => p.IsImposter);

        public GameService(
            WordService wordService,
            IJSRuntime jsRuntime,
            LocalizationService localization,
            AudioHapticService audioHaptic)
        {
            _wordService = wordService;
            _jsRuntime = jsRuntime;
            _localization = localization;
            _audioHaptic = audioHaptic;
        }

        public int GetMaxImposters() => Players.Count >= MinPlayersForTwoImposters ? 2 : 1;

        public int GetEffectiveImposterCount() => Math.Clamp(Settings.ImposterCount, 1, GetMaxImposters());

        public int GetRoundDurationSeconds()
        {
            if (Settings.AutoTimePerPlayer)
            {
                return Math.Max(1, Players.Count) * 60;
            }

            return Math.Clamp(Settings.RoundDurationMinutes, MinRoundMinutes, MaxRoundMinutes) * 60;
        }

        public int GetActiveWordPoolCount() =>
            _wordService.GetWordCount(Settings.Difficulty, Settings.SelectedCategoryIds, Settings.Language);

        /// <summary>
        /// Assigns roles to all players: 1 or 2 imposters based on settings, others receive the word.
        /// </summary>
        public void AssignRoles()
        {
            if (Players.Count < MinPlayers) return;

            var entry = _wordService.GetRandomWord(Settings.Difficulty, Settings.SelectedCategoryIds, Settings.Language);
            CurrentWord = entry.Word;
            CurrentCategoryName = entry.CategoryName;
            CurrentCategoryIcon = entry.CategoryIcon;
            CurrentDifficulty = entry.Difficulty;

            ActiveImposterCount = GetEffectiveImposterCount();

            var indices = Enumerable.Range(0, Players.Count).ToArray();
            for (int i = indices.Length - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            var imposterIndices = new HashSet<int>(indices.Take(ActiveImposterCount));

            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                player.IsImposter = imposterIndices.Contains(i);
                player.Word = player.IsImposter ? string.Empty : entry.Word;
                player.HasViewedCard = false;
            }

            IsGameInProgress = true;
            OnGameStateChanged?.Invoke();
        }

        /// <summary>
        /// Clears role assignments and round state while keeping the player roster and settings.
        /// </summary>
        public void ResetRound()
        {
            CurrentWord = string.Empty;
            CurrentCategoryName = string.Empty;
            CurrentCategoryIcon = string.Empty;
            ActiveImposterCount = 0;
            IsGameInProgress = false;

            foreach (var player in Players)
            {
                player.IsImposter = false;
                player.Word = string.Empty;
                player.HasViewedCard = false;
            }

            OnGameStateChanged?.Invoke();
        }

        public async Task ToggleDarkModeAsync()
        {
            Settings.DarkMode = !Settings.DarkMode;
            await SaveSettingsAsync();
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

                        var migrated = false;
                        for (int i = 0; i < validPlayers.Count; i++)
                        {
                            if (!IsValidAvatarUri(validPlayers[i].Uri))
                            {
                                validPlayers[i].Uri = GetAvatarUri(validPlayers[i].Id > 0 ? validPlayers[i].Id : i + 1);
                                migrated = true;
                            }
                        }

                        Players.Clear();
                        Players.AddRange(validPlayers);

                        if (migrated)
                        {
                            await SavePlayersAsync();
                        }
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

        public async Task LoadSettingsAsync()
        {
            if (_hasLoadedSettings)
            {
                await ApplyClientPreferencesAsync();
                return;
            }

            try
            {
                var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", SettingsStorageKey);
                if (!string.IsNullOrEmpty(json))
                {
                    var stored = JsonSerializer.Deserialize<GameSettings>(json);
                    if (stored is not null)
                    {
                        stored.Language = LocalizationService.NormalizeLanguage(stored.Language);
                        _localization.SetLanguage(stored.Language);

                        var validCategoryIds = new HashSet<string>(
                            _wordService.GetCategories(stored.Language).Select(c => c.Id),
                            StringComparer.OrdinalIgnoreCase);

                        stored.Difficulty = Enum.IsDefined(stored.Difficulty)
                            ? stored.Difficulty
                            : WordDifficulty.Medium;
                        stored.SelectedCategoryIds = (stored.SelectedCategoryIds ?? new List<string>())
                            .Where(id => validCategoryIds.Contains(id))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
                        stored.ImposterCount = Math.Clamp(stored.ImposterCount, 1, 2);
                        stored.RoundDurationMinutes = Math.Clamp(stored.RoundDurationMinutes, MinRoundMinutes, MaxRoundMinutes);
                        Settings = stored;
                    }
                }

                _localization.SetLanguage(Settings.Language);
                _hasLoadedSettings = true;
                await ApplyClientPreferencesAsync();
                OnSettingsChanged?.Invoke();
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
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        public async Task SaveSettingsAsync()
        {
            Settings.Language = LocalizationService.NormalizeLanguage(Settings.Language);
            _localization.SetLanguage(Settings.Language);

            if (!Enum.IsDefined(Settings.Difficulty))
            {
                Settings.Difficulty = WordDifficulty.Medium;
            }

            Settings.ImposterCount = Math.Clamp(Settings.ImposterCount, 1, 2);
            Settings.RoundDurationMinutes = Math.Clamp(Settings.RoundDurationMinutes, MinRoundMinutes, MaxRoundMinutes);

            await ApplyClientPreferencesAsync();
            OnSettingsChanged?.Invoke();

            try
            {
                var json = JsonSerializer.Serialize(Settings);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", SettingsStorageKey, json);
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
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private async Task ApplyClientPreferencesAsync()
        {
            await _audioHaptic.ApplyPreferencesAsync(
                Settings.DarkMode,
                _localization.Language,
                _localization.Direction);
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

        public List<Player> ListNewPlayers() => Players;

        public Player? GetImposter() => Players.FirstOrDefault(p => p.IsImposter);

        public List<Player> GetImposters() => Players.Where(p => p.IsImposter).ToList();
    }
}
