using ImposterGame.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ImposterGame.Services
{
    public class WordService
    {
        private static readonly ConcurrentDictionary<string, IReadOnlyList<WordCategory>> CachedByLanguage = new(StringComparer.OrdinalIgnoreCase);

        private readonly IWebHostEnvironment _environment;
        private readonly Queue<WordEntry> _remainingWords = new();
        private string _activeSelectionKey = string.Empty;
        private string? _lastWord;

        public WordService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IReadOnlyList<WordCategory> GetCategories(string? language = "fa")
        {
            var lang = LocalizationService.NormalizeLanguage(language);
            return CachedByLanguage.GetOrAdd(lang, l => LoadCategories(_environment, l));
        }

        public int GetTotalWordCount(string? language = "fa") =>
            GetCategories(language).Sum(c => c.TotalCount);

        public int GetWordCount(
            WordDifficulty difficulty,
            IReadOnlyCollection<string>? selectedCategoryIds,
            string? language = "fa")
        {
            return GetEntriesForSelection(difficulty, selectedCategoryIds, language).Count;
        }

        /// <summary>
        /// Returns a random word strictly matching the selected language, difficulty level, and categories,
        /// without repeating until the full pool has been exhausted.
        /// </summary>
        public WordEntry GetRandomWord(
            WordDifficulty difficulty = WordDifficulty.Medium,
            IReadOnlyCollection<string>? selectedCategoryIds = null,
            string? language = "fa")
        {
            var lang = LocalizationService.NormalizeLanguage(language);
            var selectionKey = BuildSelectionKey(lang, difficulty, selectedCategoryIds);

            if (selectionKey != _activeSelectionKey || _remainingWords.Count == 0)
            {
                _activeSelectionKey = selectionKey;
                RefillShuffledQueue(difficulty, selectedCategoryIds, lang);
            }

            if (_remainingWords.Count == 0)
            {
                return new WordEntry(string.Empty, string.Empty, string.Empty, string.Empty, difficulty);
            }

            var next = _remainingWords.Dequeue();
            _lastWord = next.Word;
            return next;
        }

        private List<WordEntry> GetEntriesForSelection(
            WordDifficulty difficulty,
            IReadOnlyCollection<string>? selectedCategoryIds,
            string? language)
        {
            var categories = GetCategories(language);
            IEnumerable<WordCategory> matchingCategories;

            if (selectedCategoryIds is { Count: > 0 })
            {
                var idSet = new HashSet<string>(selectedCategoryIds, StringComparer.OrdinalIgnoreCase);
                matchingCategories = categories.Where(c => idSet.Contains(c.Id)).ToList();

                if (!matchingCategories.Any())
                {
                    matchingCategories = categories;
                }
            }
            else
            {
                matchingCategories = categories;
            }

            return matchingCategories
                .SelectMany(c => c.GetWords(difficulty)
                    .Select(w => new WordEntry(w, c.Id, c.Name, c.Icon, difficulty)))
                .ToList();
        }

        private void RefillShuffledQueue(
            WordDifficulty difficulty,
            IReadOnlyCollection<string>? selectedCategoryIds,
            string language)
        {
            _remainingWords.Clear();
            var pool = GetEntriesForSelection(difficulty, selectedCategoryIds, language);
            if (pool.Count == 0)
            {
                return;
            }

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            if (pool.Count > 1 && pool[0].Word == _lastWord)
            {
                int swapIndex = Random.Shared.Next(1, pool.Count);
                (pool[0], pool[swapIndex]) = (pool[swapIndex], pool[0]);
            }

            foreach (var entry in pool)
            {
                _remainingWords.Enqueue(entry);
            }
        }

        private static string BuildSelectionKey(
            string language,
            WordDifficulty difficulty,
            IReadOnlyCollection<string>? selectedCategoryIds)
        {
            var categoryPart = selectedCategoryIds is null || selectedCategoryIds.Count == 0
                ? "*"
                : string.Join("|", selectedCategoryIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase));

            return $"{language}:{(int)difficulty}:{categoryPart}";
        }

        private static IReadOnlyList<WordCategory> LoadCategories(IWebHostEnvironment environment, string language)
        {
            var fileName = language switch
            {
                "de" => "words_de.json",
                "en" => "words_en.json",
                _ => "words.json"
            };

            var candidates = new[]
            {
                Path.Combine(environment.ContentRootPath, "Data", fileName),
                Path.Combine(AppContext.BaseDirectory, "Data", fileName)
            };

            var filePath = candidates.FirstOrDefault(File.Exists);
            if (filePath is not null)
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var categories = JsonSerializer.Deserialize<List<WordCategory>>(
                        json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (categories is { Count: > 0 })
                    {
                        return categories
                            .Where(c => !string.IsNullOrWhiteSpace(c.Id) && c.TotalCount > 0)
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading {fileName}: {ex.Message}");
                }
            }

            return CreateFallbackCategories(language);
        }

        private static IReadOnlyList<WordCategory> CreateFallbackCategories(string language) => language switch
        {
            "de" => new List<WordCategory>
            {
                new()
                {
                    Id = "general",
                    Name = "Allgemein",
                    Icon = "🎲",
                    Easy = new List<string> { "🍎 Apfel", "🏠 Haus", "🚗 Auto", "🌳 Baum" },
                    Medium = new List<string> { "🌋 Vulkan", "🧭 Kompass", "♟️ Schach", "🎬 Kino" },
                    Hard = new List<string> { "⏳ Nostalgie", "🪞 Illusion", "🗼 Leuchtturm", "🤖 Künstliche Intelligenz" }
                }
            },
            "en" => new List<WordCategory>
            {
                new()
                {
                    Id = "general",
                    Name = "General",
                    Icon = "🎲",
                    Easy = new List<string> { "🍎 Apple", "🏠 House", "🚗 Car", "🌳 Tree" },
                    Medium = new List<string> { "🌋 Volcano", "🧭 Compass", "♟️ Chess", "🎬 Cinema" },
                    Hard = new List<string> { "⏳ Nostalgia", "🪞 Illusion", "🗼 Lighthouse", "🤖 Artificial Intelligence" }
                }
            },
            _ => new List<WordCategory>
            {
                new()
                {
                    Id = "general",
                    Name = "عمومی",
                    Icon = "🎲",
                    Easy = new List<string> { "🍎 سیب", "🏠 خانه", "🚗 ماشین", "🌳 درخت" },
                    Medium = new List<string> { "🌋 آتشفشان", "🧭 قطب‌نما", "♟️ شطرنج", "🎬 سینما" },
                    Hard = new List<string> { "⏳ نوستالژی", "🪞 توهم", "🗼 فانوس دریایی", "🤖 هوش مصنوعی" }
                }
            }
        };
    }
}
