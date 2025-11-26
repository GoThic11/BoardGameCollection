using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using BoardGameCollection.Commands;
using BoardGameCollection.Models;
using BoardGameCollection.Services;
using BoardGameCollection.Views;

namespace BoardGameCollection.ViewModels
{
    public class StatusCountItem
    {
        public GameStatus Status { get; set; }
        public int Count { get; set; }
    }

    public class MainViewModel : BaseViewModel
    {
        private readonly IGameService _gameService;

        // Инициализация коллекций
        public ObservableCollection<Game> Games { get; set; } = new ObservableCollection<Game>();
        public ObservableCollection<Tag> AvailableTags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> SelectedTags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<StatusCountItem> GamesByStatus { get; set; } = new ObservableCollection<StatusCountItem>();

        // Свойства для фильтрации
        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                SetProperty(ref _searchTerm, value);
                ApplyFilters();
            }
        }

        private Genre? _selectedGenre = null;
        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                SetProperty(ref _selectedGenre, value);
                ApplyFilters();
            }
        }

        private Difficulty? _selectedDifficulty = null;
        public Difficulty? SelectedDifficulty
        {
            get => _selectedDifficulty;
            set
            {
                SetProperty(ref _selectedDifficulty, value);
                ApplyFilters();
            }
        }

        private int? _minPlayersFilter = null;
        public int? MinPlayersFilter
        {
            get => _minPlayersFilter;
            set
            {
                SetProperty(ref _minPlayersFilter, value);
                ApplyFilters();
            }
        }

        private int? _maxPlayersFilter = null;
        public int? MaxPlayersFilter
        {
            get => _maxPlayersFilter;
            set
            {
                SetProperty(ref _maxPlayersFilter, value);
                ApplyFilters();
            }
        }

        private int? _minPlayTimeFilter = null;
        public int? MinPlayTimeFilter
        {
            get => _minPlayTimeFilter;
            set
            {
                SetProperty(ref _minPlayTimeFilter, value);
                ApplyFilters();
            }
        }

        private int? _maxPlayTimeFilter = null;
        public int? MaxPlayTimeFilter
        {
            get => _maxPlayTimeFilter;
            set
            {
                SetProperty(ref _maxPlayTimeFilter, value);
                ApplyFilters();
            }
        }

        private GameStatus? _selectedStatus = null;
        public GameStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                SetProperty(ref _selectedStatus, value);
                ApplyFilters();
            }
        }

        // Статистика
        private int _totalGames;
        public int TotalGames
        {
            get => _totalGames;
            set => SetProperty(ref _totalGames, value);
        }

        private int _unplayedGamesCount;
        public int UnplayedGamesCount
        {
            get => _unplayedGamesCount;
            set => SetProperty(ref _unplayedGamesCount, value);
        }

        // Команды
        public ICommand AddGameCommand { get; }
        public ICommand EditGameCommand { get; }
        public ICommand DeleteGameCommand { get; }
        public ICommand RecordSessionCommand { get; }
        public ICommand ShowSessionHistoryCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ShowRecommendationsCommand { get; }
        public ICommand ResetFiltersCommand { get; }

        // Выбранная игра для действий
        private Game _selectedGame;
        public Game SelectedGame
        {
            get => _selectedGame;
            set => SetProperty(ref _selectedGame, value);
        }

        public MainViewModel(IGameService gameService)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));

            // Инициализация команд
            AddGameCommand = new RelayCommand(OnAddGame);
            EditGameCommand = new RelayCommand(OnEditGame, _ => SelectedGame != null);
            DeleteGameCommand = new RelayCommand(OnDeleteGame, _ => SelectedGame != null);
            RecordSessionCommand = new RelayCommand(OnRecordSession, _ => SelectedGame != null);
            ShowSessionHistoryCommand = new RelayCommand(OnShowSessionHistory, _ => SelectedGame != null);
            RefreshCommand = new RelayCommand(_ => LoadData());
            ShowRecommendationsCommand = new RelayCommand(_ => ShowRecommendations());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());

            // Инициализация данных
            LoadData();
        }

        public void LoadData()
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                System.Diagnostics.Debug.WriteLine("Начало загрузки данных");

                // Сброс фильтров для загрузки всех игр
                ResetFiltersSilent();

                // Загрузка всех игр
                var allGames = _gameService.GetAllGames() ?? new List<Game>();
                //System.Diagnostics.Debug.WriteLine($"Загружено игр: {allGames.Count}");

                UpdateGamesCollection(allGames);

                // Загрузка тегов
                LoadTags();

                // Загрузка статистики
                UpdateStatistics();

                System.Diagnostics.Debug.WriteLine("Данные успешно загружены");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при загрузке данных: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadTags()
        {
            AvailableTags.Clear();
            var allGames = _gameService.GetAllGames();

            if (allGames != null)
            {
                var tags = allGames
                    .SelectMany(g => g.GameTags)
                    .Select(gt => gt.Tag)
                    .Where(t => t != null)
                    .DistinctBy(t => t.Id)
                    .ToList();

                foreach (var tag in tags)
                {
                    AvailableTags.Add(tag);
                }

                System.Diagnostics.Debug.WriteLine($"Загружено тегов: {AvailableTags.Count}");
            }
        }

        private void UpdateStatistics()
        {
            TotalGames = _gameService.GetTotalGamesCount();
            System.Diagnostics.Debug.WriteLine($"Всего игр: {TotalGames}");

            // Обновление статистики по статусам
            UpdateStatusStatistics(_gameService.GetAllGames());

            UnplayedGamesCount = _gameService.GetUnplayedGames().Count();
            System.Diagnostics.Debug.WriteLine($"Непроверенных игр: {UnplayedGamesCount}");
        }

        private void UpdateStatusStatistics(IEnumerable<Game> games)
        {
            // Очистка текущей коллекции
            GamesByStatus.Clear();

            // Группировка по статусам и заполнение коллекции
            var statusGroups = games
                .GroupBy(g => g.Status)
                .Select(g => new StatusCountItem
                {
                    Status = g.Key,
                    Count = g.Count()
                });

            foreach (var item in statusGroups)
            {
                GamesByStatus.Add(item);
                System.Diagnostics.Debug.WriteLine($"Статус: {item.Status}, Количество: {item.Count}");
            }
        }

        private void UpdateGamesCollection(IEnumerable<Game> games)
        {
            Games.Clear();
            System.Diagnostics.Debug.WriteLine($"Очистка коллекции. Добавление {games.Count()} игр");

            foreach (var game in games)
            {
                Games.Add(game);
                System.Diagnostics.Debug.WriteLine($"Добавлена игра: {game.Title}, ID: {game.Id}, Статус: {game.Status}");
            }

            System.Diagnostics.Debug.WriteLine($"Всего игр в коллекции: {Games.Count}");
        }

        public void ApplyFilters()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = null;

            try
            {
                System.Diagnostics.Debug.WriteLine("Применение фильтров");
                System.Diagnostics.Debug.WriteLine($"SearchTerm: {SearchTerm}");
                System.Diagnostics.Debug.WriteLine($"SelectedGenre: {SelectedGenre}");
                System.Diagnostics.Debug.WriteLine($"SelectedDifficulty: {SelectedDifficulty}");
                System.Diagnostics.Debug.WriteLine($"MinPlayersFilter: {MinPlayersFilter}");
                System.Diagnostics.Debug.WriteLine($"MaxPlayersFilter: {MaxPlayersFilter}");
                System.Diagnostics.Debug.WriteLine($"MinPlayTimeFilter: {MinPlayTimeFilter}");
                System.Diagnostics.Debug.WriteLine($"MaxPlayTimeFilter: {MaxPlayTimeFilter}");
                System.Diagnostics.Debug.WriteLine($"SelectedStatus: {SelectedStatus}");
                System.Diagnostics.Debug.WriteLine($"SelectedTags.Count: {SelectedTags.Count}");

                // Формируем список выбранных тегов
                List<string> selectedTagNames = null;
                if (SelectedTags.Any())
                {
                    selectedTagNames = SelectedTags.Select(t => t.Name).ToList();
                    System.Diagnostics.Debug.WriteLine($"Выбранные теги: {string.Join(", ", selectedTagNames)}");
                }

                var filteredGames = _gameService.GetGamesByFilters(
                    searchTerm: !string.IsNullOrWhiteSpace(SearchTerm) ? SearchTerm.Trim() : null,
                    genre: SelectedGenre,
                    difficulty: SelectedDifficulty,
                    minPlayers: MinPlayersFilter,
                    maxPlayers: MaxPlayersFilter,
                    minPlayTime: MinPlayTimeFilter,
                    maxPlayTime: MaxPlayTimeFilter,
                    status: SelectedStatus,
                    tags: selectedTagNames
                );

                // Обновление отображаемых игр
                UpdateGamesCollection(filteredGames);

                // Обновление статистики для отфильтрованных игр
                UpdateFilteredStatistics(filteredGames);

                System.Diagnostics.Debug.WriteLine($"После фильтрации осталось игр: {filteredGames.Count()}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при применении фильтров: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка применения фильтров: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateFilteredStatistics(IEnumerable<Game> filteredGames)
        {
            TotalGames = filteredGames.Count();

            // Обновление статистики по статусам для отфильтрованных игр
            UpdateStatusStatistics(filteredGames);

            UnplayedGamesCount = filteredGames.Count(g => g.Sessions == null || !g.Sessions.Any());
        }

        private void ResetFiltersSilent()
        {
            _searchTerm = string.Empty;
            _selectedGenre = null;
            _selectedDifficulty = null;
            _minPlayersFilter = null;
            _maxPlayersFilter = null;
            _minPlayTimeFilter = null;
            _maxPlayTimeFilter = null;
            _selectedStatus = null;

            // Создаем новый экземпляр для очистки всех тегов
            SelectedTags = new ObservableCollection<Tag>();

            System.Diagnostics.Debug.WriteLine("Фильтры успешно сброшены");
        }

        private void ResetFilters()
        {
            ResetFiltersSilent();

            // Принудительное обновление привязок
            OnPropertyChanged(nameof(SearchTerm));
            OnPropertyChanged(nameof(SelectedGenre));
            OnPropertyChanged(nameof(SelectedDifficulty));
            OnPropertyChanged(nameof(MinPlayersFilter));
            OnPropertyChanged(nameof(MaxPlayersFilter));
            OnPropertyChanged(nameof(MinPlayTimeFilter));
            OnPropertyChanged(nameof(MaxPlayTimeFilter));
            OnPropertyChanged(nameof(SelectedStatus));

            // Применение сброшенных фильтров
            ApplyFilters();
        }

        private void OnAddGame(object parameter)
        {
            try
            {
                var gameWindow = new GameWindow();
                gameWindow.DataContext = new GameViewModel(_gameService, null);
                bool? result = gameWindow.ShowDialog();

                if (result == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при добавлении игры: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления игры: {ex.Message}");
            }
        }

        private void OnEditGame(object parameter)
        {
            if (SelectedGame == null) return;

            try
            {
                var gameWindow = new GameWindow();
                gameWindow.DataContext = new GameViewModel(_gameService, SelectedGame.Id);
                bool? result = gameWindow.ShowDialog();

                if (result == true)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при редактировании игры: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка редактирования игры: {ex.Message}");
            }
        }

        private void OnDeleteGame(object parameter)
        {
            if (SelectedGame == null) return;

            try
            {
                // В реальном приложении здесь нужно запросить подтверждение
                _gameService.DeleteGame(SelectedGame.Id);
                LoadData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при удалении игры: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления игры: {ex.Message}");
            }
        }

        private void OnRecordSession(object parameter)
        {
            if (SelectedGame == null) return;

            try
            {
                var sessionWindow = new SessionWindow();
                sessionWindow.DataContext = new SessionViewModel(_gameService, SelectedGame.Id);
                sessionWindow.ShowDialog();

                LoadData();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при записи игровой сессии: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка записи игровой сессии: {ex.Message}");
            }
        }

        private void OnShowSessionHistory(object parameter)
        {
            if (SelectedGame == null) return;

            try
            {
                var sessionWindow = new SessionWindow();
                sessionWindow.DataContext = new SessionViewModel(_gameService, SelectedGame.Id);
                sessionWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при отображении истории сессий: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка отображения истории сессий: {ex.Message}");
            }
        }

        private void ShowRecommendations()
        {
            try
            {
                var recommendedGames = _gameService.GetRecommendedGames();
                if (recommendedGames.Any())
                {
                    UpdateGamesCollection(recommendedGames);
                    UpdateFilteredStatistics(recommendedGames);
                }
                else
                {
                    ErrorMessage = "Нет рекомендаций для отображения";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при получении рекомендаций: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка получения рекомендаций: {ex.Message}");
            }
        }
    }
}