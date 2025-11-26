using BoardGameCollection.Data;
using BoardGameCollection.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BoardGameCollection.Services
{
    public class GameService : IGameService
    {
        private readonly BoardGameContext _context;

        public GameService(BoardGameContext context)
        {
            _context = context;
        }

        public IEnumerable<Game> GetAllGames()
        {
            return _context.Games
                .Include(g => g.Sessions)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .ToList();
        }

        public Game GetGameById(int id)
        {
            return _context.Games
                .Include(g => g.Sessions)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .FirstOrDefault(g => g.Id == id);
        }

        public int AddGame(Game game)
        {
            _context.Games.Add(game);
            _context.SaveChanges();
            return game.Id;
        }

        public void UpdateGame(Game game)
        {
            var existingGame = _context.Games.Find(game.Id);
            if (existingGame != null)
            {
                _context.Entry(existingGame).CurrentValues.SetValues(game);
                _context.SaveChanges();
            }
        }

        public void DeleteGame(int id)
        {
            var game = _context.Games.Find(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Game> GetGamesByFilters(string searchTerm = null,
                                                 Genre? genre = null,
                                                 Difficulty? difficulty = null,
                                                 int? minPlayers = null,
                                                 int? maxPlayers = null,
                                                 int? minPlayTime = null,
                                                 int? maxPlayTime = null,
                                                 GameStatus? status = null,
                                                 List<string> tags = null)
        {
            var query = _context.Games
                .Include(g => g.Sessions)
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .AsQueryable();

            // Поиск по названию, издателю или тегам
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                query = query.Where(g =>
                    g.Title.ToLower().Contains(searchTerm) ||
                    (g.Publisher != null && g.Publisher.ToLower().Contains(searchTerm)) ||
                    (g.GameTags != null && g.GameTags.Any(gt => gt.Tag != null && gt.Tag.Name.ToLower().Contains(searchTerm))));
            }

            // Фильтрация по жанру
            if (genre.HasValue)
            {
                query = query.Where(g => g.Genre == genre.Value);
            }

            // Фильтрация по сложности
            if (difficulty.HasValue)
            {
                query = query.Where(g => g.Difficulty == difficulty.Value);
            }

            // Фильтрация по количеству игроков
            if (minPlayers.HasValue)
            {
                query = query.Where(g => g.MaxPlayers >= minPlayers.Value);
            }

            if (maxPlayers.HasValue)
            {
                query = query.Where(g => g.MinPlayers <= maxPlayers.Value);
            }

            // Фильтрация по времени игры
            if (minPlayTime.HasValue)
            {
                query = query.Where(g => g.PlayTime >= minPlayTime.Value);
            }

            if (maxPlayTime.HasValue)
            {
                query = query.Where(g => g.PlayTime <= maxPlayTime.Value);
            }

            // Фильтрация по статусу
            if (status.HasValue)
            {
                query = query.Where(g => g.Status == status.Value);
            }

            // Фильтрация по тегам
            if (tags != null && tags.Any())
            {
                query = query.Where(g => g.GameTags != null && g.GameTags
                    .Any(gt => gt.Tag != null && tags.Contains(gt.Tag.Name)));
            }

            var result = query.ToList();
            Debug.WriteLine($"Количество игр после фильтрации: {result.Count}");
            return result;
        }

        public IEnumerable<Game> GetUnplayedGames()
        {
            return _context.Games
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .Where(g => g.Sessions == null || !g.Sessions.Any())
                .ToList();
        }

        public void AddGameSession(GameSession session)
        {
            _context.GameSessions.Add(session);

            // Обновление даты последней игры
            var game = _context.Games.Find(session.GameId);
            if (game != null)
            {
                game.LastPlayed = session.SessionDate;
                _context.SaveChanges();
            }
        }

        public void UpdateGameLastPlayed(int gameId, DateTime date)
        {
            var game = _context.Games.Find(gameId);
            if (game != null)
            {
                game.LastPlayed = date;
                _context.SaveChanges();
            }
        }

        public void UpdateGameTags(int gameId, List<int> tagIds)
        {
            var existingTags = _context.GameTags.Where(gt => gt.GameId == gameId).ToList();
            _context.GameTags.RemoveRange(existingTags);

            foreach (var tagId in tagIds)
            {
                _context.GameTags.Add(new GameTag { GameId = gameId, TagId = tagId });
            }

            _context.SaveChanges();
        }

        public IEnumerable<Game> GetRecommendedGames()
        {
            var topRatedGames = _context.Games
                .Where(g => g.PersonalRating >= 8)
                .OrderByDescending(g => g.PersonalRating)
                .Take(3)
                .ToList();

            if (!topRatedGames.Any())
            {
                return GetUnplayedGames().Take(5).ToList();
            }

            var recommendedGenres = topRatedGames.Select(g => g.Genre).Distinct().ToList();

            var recommendedGames = _context.Games
                .Include(g => g.GameTags)
                    .ThenInclude(gt => gt.Tag)
                .Where(g => recommendedGenres.Contains(g.Genre) && g.PersonalRating >= 7)
                .OrderByDescending(g => g.PersonalRating)
                .Take(5)
                .ToList();

            return recommendedGames;
        }

        public int GetTotalGamesCount()
        {
            return _context.Games.Count();
        }

        public Dictionary<GameStatus, int> GetGamesCountByStatus()
        {
            return _context.Games
                .GroupBy(g => g.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<Genre, int> GetGamesCountByGenre()
        {
            return _context.Games
                .GroupBy(g => g.Genre)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public double GetAverageSessionRating(int gameId)
        {
            var sessions = _context.GameSessions
                .Where(s => s.GameId == gameId)
                .ToList();

            if (!sessions.Any())
                return 0;

            return sessions.Average(s => s.SessionRating);
        }

        public int GetTotalSessionsCount(int gameId)
        {
            return _context.GameSessions.Count(s => s.GameId == gameId);
        }

        public double CalculateCostPerSession(int gameId)
        {
            double baseCost = 500;
            var totalSessions = GetTotalSessionsCount(gameId);
            return totalSessions > 0 ? baseCost / totalSessions : 0;
        }
    }
}