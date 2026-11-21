namespace ImposterGame.Models
{
    public class GameSettings
    {
        /// <summary>
        /// UI and word-pool language: "fa" (Persian RTL), "de" (German LTR), or "en" (English LTR).
        /// </summary>
        public string Language { get; set; } = "fa";

        /// <summary>
        /// Enables cozy living-room dark mode.
        /// </summary>
        public bool DarkMode { get; set; } = false;

        /// <summary>
        /// Enables Web Audio sound effects and mobile vibration haptics.
        /// </summary>
        public bool SoundEnabled { get; set; } = true;

        /// <summary>
        /// Difficulty level controlling which word pool is used (Easy, Medium, Hard).
        /// </summary>
        public WordDifficulty Difficulty { get; set; } = WordDifficulty.Medium;

        /// <summary>
        /// Selected category IDs. An empty list means all categories are included.
        /// </summary>
        public List<string> SelectedCategoryIds { get; set; } = new();

        /// <summary>
        /// Desired number of imposters (1 or 2; clamped based on player count).
        /// </summary>
        public int ImposterCount { get; set; } = 1;

        /// <summary>
        /// When true, round duration is 1 minute per player.
        /// </summary>
        public bool AutoTimePerPlayer { get; set; } = true;

        /// <summary>
        /// Fixed round duration in minutes when AutoTimePerPlayer is false (1..15).
        /// </summary>
        public int RoundDurationMinutes { get; set; } = 5;

        /// <summary>
        /// When true, imposters see the category of the secret word as a hint.
        /// </summary>
        public bool ShowCategoryHintToImposter { get; set; } = false;
    }
}
