using ImposterGame.Models;

namespace ImposterGame.Services
{
    public class GameService
    {
        private readonly WordService _wordService;
        private readonly Random _random = new();

        public List<Player> Players { get; } = new();

        public GameService(WordService wordService)
        {
            _wordService = wordService;
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

        /// <summary>
        /// Removes a player from the game.
        /// </summary>
        public Task<List<Player>> DeleteUser(Player player)
        {
            if (player is not null)
            {
                Players.Remove(player);
            }

            return Task.FromResult(Players);
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
