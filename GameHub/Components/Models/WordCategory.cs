namespace ImposterGame.Models
{
    public enum WordDifficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    public class WordCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<string> Easy { get; set; } = new();
        public List<string> Medium { get; set; } = new();
        public List<string> Hard { get; set; } = new();

        public IReadOnlyList<string> GetWords(WordDifficulty difficulty) => difficulty switch
        {
            WordDifficulty.Easy => Easy,
            WordDifficulty.Medium => Medium,
            WordDifficulty.Hard => Hard,
            _ => Medium
        };

        public int TotalCount => Easy.Count + Medium.Count + Hard.Count;
    }

    public record WordEntry(
        string Word,
        string CategoryId,
        string CategoryName,
        string CategoryIcon,
        WordDifficulty Difficulty);
}
