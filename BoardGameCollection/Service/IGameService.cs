using System.Collections.Generic;
using BoardGameCollection.Models;

namespace BoardGameCollection.Services
{
    public interface IGameService
    {
        IEnumerable<Game> GetAllGames();
        Game GetGameById(int id);
        int AddGame(Game game);
        void UpdateGame(Game game);
        void DeleteGame(int id);

        IEnumerable<Game> GetGamesByFilters(string searchTerm = null,
                                           Genre? genre = null,
                                           Difficulty? difficulty = null,
                                           int? minPlayers = null,
                                           int? maxPlayers = null,
                                           int? minPlayTime = null,
                                           int? maxPlayTime = null,
                                           GameStatus? status = null,
                                           List<string> tags = null);

        IEnumerable<Game> GetUnplayedGames();
        void AddGameSession(GameSession session);
        void UpdateGameLastPlayed(int gameId, DateTime date);
        void UpdateGameTags(int gameId, List<int> tagIds);

        IEnumerable<Game> GetRecommendedGames();

        int GetTotalGamesCount();
        Dictionary<GameStatus, int> GetGamesCountByStatus();
        Dictionary<Genre, int> GetGamesCountByGenre();
        double GetAverageSessionRating(int gameId);
        int GetTotalSessionsCount(int gameId);
        double CalculateCostPerSession(int gameId);
    }
}