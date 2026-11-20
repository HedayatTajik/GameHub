using ImposterGame.Models;
using System.Text.Json;

namespace ImposterGame.Services
{
    public class WordService
    {
        private static readonly object CacheLock = new();
        private static IReadOnlyList<WordCategory>? _cachedCategories;

        private readonly Queue<WordEntry> _remainingWords = new();
        private string _activeSelectionKey = string.Empty;
        private string? _lastWord;

        public IReadOnlyList<WordCategory> Categories { get; }

        public WordService(IWebHostEnvironment environment)
        {
            Categories = LoadCategories(environment);
        }

        public int TotalWordCount => Categories.Sum(c => c.TotalCount);

        public int GetWordCount(WordDifficulty difficulty, IReadOnlyCollection<string>? selectedCategoryIds)
        {
            return GetEntriesForSelection(difficulty, selectedCategoryIds).Count;
        }

        /// <summary>
        /// Returns a random word strictly matching the selected difficulty level and categories,
        /// without repeating until the full pool has been exhausted.
        /// </summary>
        public WordEntry GetRandomWord(
            WordDifficulty difficulty = WordDifficulty.Medium,
            IReadOnlyCollection<string>? selectedCategoryIds = null)
        {
            var selectionKey = BuildSelectionKey(difficulty, selectedCategoryIds);

            if (selectionKey != _activeSelectionKey || _remainingWords.Count == 0)
            {
                _activeSelectionKey = selectionKey;
                RefillShuffledQueue(difficulty, selectedCategoryIds);
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
            IReadOnlyCollection<string>? selectedCategoryIds)
        {
            IEnumerable<WordCategory> matchingCategories;

            if (selectedCategoryIds is { Count: > 0 })
            {
                var idSet = new HashSet<string>(selectedCategoryIds, StringComparer.OrdinalIgnoreCase);
                matchingCategories = Categories.Where(c => idSet.Contains(c.Id)).ToList();

                // Fallback to all categories if saved IDs no longer match any category
                if (!matchingCategories.Any())
                {
                    matchingCategories = Categories;
                }
            }
            else
            {
                matchingCategories = Categories;
            }

            return matchingCategories
                .SelectMany(c => c.GetWords(difficulty)
                    .Select(w => new WordEntry(w, c.Id, c.Name, c.Icon, difficulty)))
                .ToList();
        }

        private void RefillShuffledQueue(
            WordDifficulty difficulty,
            IReadOnlyCollection<string>? selectedCategoryIds)
        {
            _remainingWords.Clear();
            var pool = GetEntriesForSelection(difficulty, selectedCategoryIds);
            if (pool.Count == 0)
            {
                return;
            }

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            // Avoid repeating the immediately previous word across reshuffles
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
            WordDifficulty difficulty,
            IReadOnlyCollection<string>? selectedCategoryIds)
        {
            var categoryPart = selectedCategoryIds is null || selectedCategoryIds.Count == 0
                ? "*"
                : string.Join("|", selectedCategoryIds
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase));

            return $"{(int)difficulty}:{categoryPart}";
        }

        private static IReadOnlyList<WordCategory> LoadCategories(IWebHostEnvironment environment)
        {
            if (_cachedCategories is not null)
            {
                return _cachedCategories;
            }

            lock (CacheLock)
            {
                if (_cachedCategories is not null)
                {
                    return _cachedCategories;
                }

                var candidates = new[]
                {
                    Path.Combine(environment.ContentRootPath, "Data", "words.json"),
                    Path.Combine(AppContext.BaseDirectory, "Data", "words.json")
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
                            _cachedCategories = categories
                                .Where(c => !string.IsNullOrWhiteSpace(c.Id) && c.TotalCount > 0)
                                .ToList();
                            return _cachedCategories;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading words.json: {ex.Message}");
                    }
                }

                _cachedCategories = CreateFallbackCategories();
                return _cachedCategories;
            }
        }

        private static IReadOnlyList<WordCategory> CreateFallbackCategories() => new List<WordCategory>
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
        };
    }
}
