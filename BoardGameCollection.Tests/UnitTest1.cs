using BoardGameCollection.Models;
using BoardGameCollection.Services;
using BoardGameCollection.ViewModels;
using BoardGameCollection.Views;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace BoardGameCollection.Tests
{
    public class UnitTests
    {
        private readonly Mock<IGameService> _mockGameService;
        private readonly MainViewModel _viewModel;

        public UnitTests()
        {
            _mockGameService = new Mock<IGameService>();
            _viewModel = new MainViewModel(_mockGameService.Object);
        }

        [Fact]
        public void GetGamesByFilters_Should_FilterByGenre()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { Title = "Катан", Genre = Genre.Strategy },
                new Game { Title = "Монополия", Genre = Genre.Economic },
                new Game { Title = "Каркассон", Genre = Genre.Strategy }
            };

            _mockGameService.Setup(s => s.GetAllGames()).Returns(games);
            _viewModel.LoadData(); // Загружаем данные перед фильтрацией

            // Act
            _viewModel.SelectedGenre = Genre.Strategy;
            _viewModel.ApplyFilters();

            // Assert
            Assert.Equal(2, _viewModel.Games.Count);
            Assert.All(_viewModel.Games, g => Assert.Equal(Genre.Strategy, g.Genre));
        }

        [Fact]
        public void GetGamesByFilters_Should_FilterByPlayerRange()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { Title = "Катан", MinPlayers = 3, MaxPlayers = 4 },
                new Game { Title = "Монополия", MinPlayers = 2, MaxPlayers = 8 },
                new Game { Title = "Каркассон", MinPlayers = 2, MaxPlayers = 5 }
            };

            _mockGameService.Setup(s => s.GetAllGames()).Returns(games);
            _viewModel.LoadData(); // Загружаем данные перед фильтрацией

            // Act
            _viewModel.MinPlayersFilter = 2;
            _viewModel.MaxPlayersFilter = 4;
            _viewModel.ApplyFilters();

            // Assert
            Assert.Equal(2, _viewModel.Games.Count);
            Assert.All(_viewModel.Games, g =>
                Assert.True(g.MinPlayers <= 4 && g.MaxPlayers >= 2));
        }

        [Fact]
        public void GetGamesByFilters_Should_FilterBySearchTerm()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { Title = "Катан", Publisher = "KOSMOS" },
                new Game { Title = "Монополия", Publisher = "Hasbro" },
                new Game { Title = "Каркассон", Publisher = "Hans im Glück" }
            };

            _mockGameService.Setup(s => s.GetAllGames()).Returns(games);
            _viewModel.LoadData(); // Загружаем данные перед фильтрацией

            // Act
            _viewModel.SearchTerm = "ка";
            _viewModel.ApplyFilters();

            // Assert
            Assert.Equal(2, _viewModel.Games.Count);
            Assert.All(_viewModel.Games, g =>
                Assert.True(
                    g.Title.ToLower().Contains("ка") ||
                    g.Publisher.ToLower().Contains("ка"),
                    $"Игра '{g.Title}' не содержит 'ка' в названии или издателе"
                )
            );
        }

        [Fact]
        public void MainViewModel_LoadData_ShouldInitializeGamesCollection()
        {
            // Arrange
            var mockGameService = new Mock<IGameService>();
            var testGames = new List<Game>
            {
                new Game { Id = 1, Title = "Катан" },
                new Game { Id = 2, Title = "Монополия" }
            };
            mockGameService.Setup(s => s.GetAllGames()).Returns(testGames);

            var viewModel = new MainViewModel(mockGameService.Object);

            // Act
            viewModel.LoadData();

            // Assert
            Assert.NotNull(viewModel.Games);
            Assert.Equal(2, viewModel.Games.Count);
            Assert.Equal("Катан", viewModel.Games[0].Title);
        }

        [Fact]
        public void GetUnplayedGames_Should_ReturnGamesWithoutSessions()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { Title = "Катан", Sessions = new List<GameSession>() },
                new Game { Title = "Монополия", Sessions = new List<GameSession> { new GameSession() } },
                new Game { Title = "Каркассон", Sessions = new List<GameSession>() }
            };

            _mockGameService.Setup(s => s.GetAllGames()).Returns(games);

            // Act
            _viewModel.LoadData();

            // Assert
            Assert.Equal(2, _viewModel.UnplayedGamesCount);
        }

        [Fact]
        public void AddGameCommand_Should_ExecuteSuccessfully()
        {
            // Arrange
            var mockGameWindow = new Mock<GameWindow>();

            // Act
            _viewModel.AddGameCommand.Execute(mockGameWindow.Object);

            // Assert
            Assert.True(true);
        }
    }
}