using System.Text.Json.Serialization;

namespace ImposterGame.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsImposter { get; set; } = false;

        [JsonIgnore]
        public string Word { get; set; } = string.Empty;

        [JsonIgnore]
        public bool HasViewedCard { get; set; } = false;

        public string Uri { get; set; } = string.Empty;
    }
}
