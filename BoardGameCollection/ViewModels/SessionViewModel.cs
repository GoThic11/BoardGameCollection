using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BoardGameCollection.Commands;
using BoardGameCollection.Models;
using BoardGameCollection.Services;
using BoardGameCollection.Views;

namespace BoardGameCollection.ViewModels
{
    public class SessionViewModel : BaseViewModel
    {
        private readonly IGameService _gameService;
        private readonly int _gameId;

        public Game Game { get; private set; }

        // Данные для новой сессии
        private DateTime _sessionDate = DateTime.Now;
        public DateTime SessionDate
        {
            get => _sessionDate;
            set => SetProperty(ref _sessionDate, value);
        }

        private int _playersCount;
        public int PlayersCount
        {
            get => _playersCount;
            set => SetProperty(ref _playersCount, value);
        }

        private string _results = string.Empty;
        public string Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        private int _sessionRating = 5;
        public int SessionRating
        {
            get => _sessionRating;
            set => SetProperty(ref _sessionRating, value);
        }

        // История игровых сессий
        public ObservableCollection<GameSession> Sessions { get; set; } = new ObservableCollection<GameSession>();

        // Статистика
        private double _averageSessionRating;
        public double AverageSessionRating
        {
            get => _averageSessionRating;
            set => SetProperty(ref _averageSessionRating, value);
        }

        private int _totalSessionsCount;
        public int TotalSessionsCount
        {
            get => _totalSessionsCount;
            set => SetProperty(ref _totalSessionsCount, value);
        }

        private double _costPerSession;
        public double CostPerSession
        {
            get => _costPerSession;
            set => SetProperty(ref _costPerSession, value);
        }

        // Команды
        public ICommand AddSessionCommand { get; }
        public ICommand CloseCommand { get; }

        public SessionViewModel(IGameService gameService, int gameId)
        {
            _gameService = gameService;
            _gameId = gameId;

            // Команды
            AddSessionCommand = new RelayCommand(OnAddSession);
            CloseCommand = new RelayCommand(OnClose);

            // Загрузка данных
            LoadData();
        }

        private void LoadData()
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                // Загрузка игры
                Game = _gameService.GetGameById(_gameId);
                if (Game == null)
                {
                    ErrorMessage = "Игра не найдена";
                    return;
                }

                // Загрузка игровых сессий
                Sessions.Clear();
                foreach (var session in Game.Sessions.OrderBy(s => s.SessionDate))
                {
                    Sessions.Add(session);
                }

                // Расчет статистики
                AverageSessionRating = _gameService.GetAverageSessionRating(_gameId);
                TotalSessionsCount = _gameService.GetTotalSessionsCount(_gameId);
                CostPerSession = CalculateCostPerSession();

                // Значение по умолчанию для количества игроков
                PlayersCount = Game.MinPlayers + (Game.MaxPlayers - Game.MinPlayers) / 2;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке данных: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private double CalculateCostPerSession()
        {
            // Оценочная стоимость игры (можно сделать более сложную логику)
            double baseCost = 500;
            return TotalSessionsCount > 0 ? baseCost / TotalSessionsCount : 0;
        }

        private void OnAddSession(object parameter)
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                var session = new GameSession
                {
                    GameId = _gameId,
                    SessionDate = SessionDate,
                    PlayersCount = PlayersCount,
                    Results = Results,
                    SessionRating = SessionRating
                };

                _gameService.AddGameSession(session);

                // Обновление данных
                LoadData();

                // Очистка полей для новой сессии
                SessionDate = DateTime.Now;
                PlayersCount = Game.MinPlayers + (Game.MaxPlayers - Game.MinPlayers) / 2;
                Results = string.Empty;
                SessionRating = 5;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при добавлении сессии: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnClose(object parameter)
        {
            if (parameter is System.Windows.Window window)
            {
                window.Close();
            }
        }
    }
}