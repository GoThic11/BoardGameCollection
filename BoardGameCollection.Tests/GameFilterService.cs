using BoardGameCollection.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGameCollection.Tests.Services
{
    public class GameFilterService
    {
        public IEnumerable<Game> FilterByGenre(IEnumerable<Game> games, Genre? genre)
        {
            if (genre.HasValue)
            {
                return games.Where(g => g.Genre == genre.Value);
            }
            return games;
        }

        public IEnumerable<Game> FilterByPlayerRange(IEnumerable<Game> games, int? minPlayers, int? maxPlayers)
        {
            var query = games.AsQueryable();

            if (minPlayers.HasValue)
            {
                query = query.Where(g => g.MaxPlayers >= minPlayers.Value);
            }

            if (maxPlayers.HasValue)
            {
                query = query.Where(g => g.MinPlayers <= maxPlayers.Value);
            }

            return query.ToList();
        }

        public IEnumerable<Game> FilterBySearchTerm(IEnumerable<Game> games, string? searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                return games.Where(g =>
                    g.Title.ToLower().Contains(searchTerm) ||
                    g.Publisher.ToLower().Contains(searchTerm));
            }
            return games;
        }

        public int CountUnplayedGames(IEnumerable<Game> games)
        {
            return games.Count(g => g.Sessions == null || !g.Sessions.Any());
        }
    }
}