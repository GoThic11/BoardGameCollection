using BoardGameCollection.Models;
using BoardGameCollection.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BoardGameCollection.Tests
{
    public class GameFilterServiceTests
    {
        private readonly GameFilterService _filterService;
        private readonly List<Game> _testGames;

        public GameFilterServiceTests()
        {
            _filterService = new GameFilterService();
            _testGames = new List<Game>
            {
                new Game { Id = 1, Title = "Катан", Genre = Genre.Strategy, MinPlayers = 3, MaxPlayers = 4, Publisher = "KOSMOS" },
                new Game { Id = 2, Title = "Монополия", Genre = Genre.Economic, MinPlayers = 2, MaxPlayers = 8, Publisher = "Hasbro" },
                new Game { Id = 3, Title = "Каркассон", Genre = Genre.Strategy, MinPlayers = 2, MaxPlayers = 5, Publisher = "Hans im Glück" },
                new Game { Id = 4, Title = "Шахматы", Genre = Genre.Strategy, MinPlayers = 2, MaxPlayers = 2, Publisher = "Русский стиль" }
            };
        }

        [Fact]
        public void FilterByGenre_ShouldReturnCorrectGames()
        {
            // Arrange
            var expectedCount = 3; // Катан, Каркассон, Шахматы

            // Act
            var result = _filterService.FilterByGenre(_testGames, Genre.Strategy);

            // Assert
            Assert.Equal(expectedCount, result.Count());
            Assert.All(result, g => Assert.Equal(Genre.Strategy, g.Genre));
        }


        [Fact]
        public void FilterBySearchTerm_ShouldReturnCorrectGames()
        {
            // Arrange
            var expectedCount = 2; // Катан и Каркассон содержат "ка"

            // Act
            var result = _filterService.FilterBySearchTerm(_testGames, "ка");

            // Assert
            Assert.Equal(expectedCount, result.Count());
            Assert.All(result, g =>
            {
                Assert.True(
                    g.Title.ToLower().Contains("ка") ||
                    g.Publisher.ToLower().Contains("ка")
                );
            });
        }

        [Fact]
        public void CountUnplayedGames_ShouldReturnCorrectCount()
        {
            // Arrange
            var gamesWithSessions = new List<Game>
            {
                new Game { Title = "Катан", Sessions = new List<GameSession>() },
                new Game { Title = "Монополия", Sessions = new List<GameSession> { new GameSession() } },
                new Game { Title = "Каркассон", Sessions = new List<GameSession>() }
            };
            var expectedCount = 2; // Только Монополия имеет сессии

            // Act
            var result = _filterService.CountUnplayedGames(gamesWithSessions);

            // Assert
            Assert.Equal(expectedCount, result);
        }

        [Fact]
        public void FilterByGenre_WithNullGenre_ShouldReturnAllGames()
        {
            // Arrange
            var expectedCount = _testGames.Count;

            // Act
            var result = _filterService.FilterByGenre(_testGames, null);

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }

        [Fact]
        public void FilterByPlayerRange_WithNullFilters_ShouldReturnAllGames()
        {
            // Arrange
            var expectedCount = _testGames.Count;

            // Act
            var result = _filterService.FilterByPlayerRange(_testGames, null, null);

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }

        [Fact]
        public void FilterBySearchTerm_WithNullSearchTerm_ShouldReturnAllGames()
        {
            // Arrange
            var expectedCount = _testGames.Count;

            // Act
            var result = _filterService.FilterBySearchTerm(_testGames, null);

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }

        [Fact]
        public void FilterBySearchTerm_WithEmptySearchTerm_ShouldReturnAllGames()
        {
            // Arrange
            var expectedCount = _testGames.Count;

            // Act
            var result = _filterService.FilterBySearchTerm(_testGames, "");

            // Assert
            Assert.Equal(expectedCount, result.Count());
        }
    }
}