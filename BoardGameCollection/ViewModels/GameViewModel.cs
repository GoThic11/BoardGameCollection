using BoardGameCollection.Commands;
using BoardGameCollection.Models;
using BoardGameCollection.Services;
using BoardGameCollection.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BoardGameCollection.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private readonly IGameService _gameService;
        private readonly int? _gameId;

        // Свойства игры
        public int Id { get; set; }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                SetProperty(ref _title, value);
                ValidateTitle();
            }
        }

        private string _titleValidationMessage = string.Empty;
        public string TitleValidationMessage
        {
            get => _titleValidationMessage;
            set => SetProperty(ref _titleValidationMessage, value);
        }

        private Genre _selectedGenre;
        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set => SetProperty(ref _selectedGenre, value);
        }

        private Difficulty _selectedDifficulty;
        public Difficulty SelectedDifficulty
        {
            get => _selectedDifficulty;
            set => SetProperty(ref _selectedDifficulty, value);
        }

        private int _minPlayers = 1;
        public int MinPlayers
        {
            get => _minPlayers;
            set
            {
                SetProperty(ref _minPlayers, value);
                ValidatePlayerCount();
            }
        }

        private int _maxPlayers = 4;
        public int MaxPlayers
        {
            get => _maxPlayers;
            set
            {
                SetProperty(ref _maxPlayers, value);
                ValidatePlayerCount();
            }
        }

        private string _playersValidationMessage = string.Empty;
        public string PlayersValidationMessage
        {
            get => _playersValidationMessage;
            set => SetProperty(ref _playersValidationMessage, value);
        }

        private int _playTime = 60;
        public int PlayTime
        {
            get => _playTime;
            set
            {
                SetProperty(ref _playTime, value);
                ValidatePlayTime();
            }
        }

        private string _playTimeValidationMessage = string.Empty;
        public string PlayTimeValidationMessage
        {
            get => _playTimeValidationMessage;
            set => SetProperty(ref _playTimeValidationMessage, value);
        }

        private string _publisher = string.Empty;
        public string Publisher
        {
            get => _publisher;
            set => SetProperty(ref _publisher, value);
        }

        private int _yearPublished;
        public int YearPublished
        {
            get => _yearPublished;
            set
            {
                SetProperty(ref _yearPublished, value);
                ValidateYearPublished();
            }
        }

        private string _yearValidationMessage = string.Empty;
        public string YearValidationMessage
        {
            get => _yearValidationMessage;
            set => SetProperty(ref _yearValidationMessage, value);
        }

        private double? _bggRating;
        public double? BggRating
        {
            get => _bggRating;
            set
            {
                SetProperty(ref _bggRating, value);
                ValidateBggRating();
            }
        }

        private string _bggRatingValidationMessage = string.Empty;
        public string BggRatingValidationMessage
        {
            get => _bggRatingValidationMessage;
            set => SetProperty(ref _bggRatingValidationMessage, value);
        }

        private int _personalRating = 5;
        public int PersonalRating
        {
            get => _personalRating;
            set => SetProperty(ref _personalRating, value);
        }

        private GameStatus _selectedStatus;
        public GameStatus SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        // Система тегов
        public ObservableCollection<Tag> AllTags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> SelectedTags { get; set; } = new ObservableCollection<Tag>();

        // Команды
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        public GameViewModel(IGameService gameService, int? gameId = null)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _gameId = gameId;

            // Установка значения по умолчанию для года
            _yearPublished = DateTime.Now.Year;

            // Команды
            SaveCommand = new RelayCommand(OnSave, _ => IsValid);
            CancelCommand = new RelayCommand(OnCancel);

            // Загрузка данных
            LoadData();
        }

        private void LoadData()
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                // Загрузка всех тегов
                var allGames = _gameService.GetAllGames() ?? new List<Game>();
                var tags = allGames
                    .SelectMany(g => g.GameTags ?? Enumerable.Empty<GameTag>())
                    .Select(gt => gt.Tag)
                    .Where(t => t != null)
                    .DistinctBy(t => t.Id)
                    .ToList();

                AllTags.Clear();
                foreach (var tag in tags)
                {
                    AllTags.Add(tag);
                }

                // Если редактируем существующую игру
                if (_gameId.HasValue)
                {
                    var game = _gameService.GetGameById(_gameId.Value);
                    if (game != null)
                    {
                        Id = game.Id;
                        Title = game.Title ?? string.Empty;
                        SelectedGenre = game.Genre;
                        SelectedDifficulty = game.Difficulty;
                        MinPlayers = game.MinPlayers;
                        MaxPlayers = game.MaxPlayers;
                        PlayTime = game.PlayTime;
                        Publisher = game.Publisher ?? string.Empty;
                        YearPublished = game.YearPublished;
                        BggRating = game.BggRating;
                        PersonalRating = game.PersonalRating;
                        SelectedStatus = game.Status;

                        // Загрузка выбранных тегов
                        SelectedTags.Clear();
                        if (game.GameTags != null)
                        {
                            foreach (var gameTag in game.GameTags)
                            {
                                var tag = AllTags.FirstOrDefault(t => t.Id == gameTag.TagId);
                                if (tag != null && !SelectedTags.Contains(tag))
                                {
                                    SelectedTags.Add(tag);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Новая игра - значения по умолчанию
                    SelectedGenre = Genre.Strategy;
                    SelectedDifficulty = Difficulty.Medium;
                    SelectedStatus = GameStatus.InCollection;
                }

                // Валидация начальных значений
                ValidateAll();
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

        private void ValidateAll()
        {
            ValidateTitle();
            ValidatePlayerCount();
            ValidatePlayTime();
            ValidateYearPublished();
            ValidateBggRating();

            IsValid = string.IsNullOrEmpty(TitleValidationMessage) &&
                      string.IsNullOrEmpty(PlayersValidationMessage) &&
                      string.IsNullOrEmpty(PlayTimeValidationMessage) &&
                      string.IsNullOrEmpty(YearValidationMessage) &&
                      string.IsNullOrEmpty(BggRatingValidationMessage);
        }

        private void ValidateTitle()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                TitleValidationMessage = "Название обязательно";
            }
            else if (Title.Length > 100)
            {
                TitleValidationMessage = "Название не должно превышать 100 символов";
            }
            else
            {
                TitleValidationMessage = string.Empty;
            }
        }

        private void ValidatePlayerCount()
        {
            string message = string.Empty;

            if (MinPlayers < 1 || MinPlayers > 20)
            {
                message = "Минимальное количество игроков должно быть от 1 до 20";
            }
            else if (MaxPlayers < 1 || MaxPlayers > 20)
            {
                message = "Максимальное количество игроков должно быть от 1 до 20";
            }
            else if (MinPlayers > MaxPlayers)
            {
                message = "Минимальное количество игроков не может быть больше максимального";
            }

            PlayersValidationMessage = message;
        }

        private void ValidatePlayTime()
        {
            if (PlayTime < 5 || PlayTime > 600)
            {
                PlayTimeValidationMessage = "Время игры должно быть от 5 до 600 минут";
            }
            else
            {
                PlayTimeValidationMessage = string.Empty;
            }
        }

        private void ValidateYearPublished()
        {
            if (YearPublished < 1900 || YearPublished > 2100)
            {
                YearValidationMessage = "Год издания должен быть в диапазоне 1900-2100";
            }
            else
            {
                YearValidationMessage = string.Empty;
            }
        }

        private void ValidateBggRating()
        {
            if (BggRating.HasValue)
            {
                if (BggRating.Value < 1 || BggRating.Value > 10)
                {
                    BggRatingValidationMessage = "Рейтинг BGG должен быть от 1 до 10";
                }
                else
                {
                    BggRatingValidationMessage = string.Empty;
                }
            }
            else
            {
                BggRatingValidationMessage = string.Empty;
            }
        }

        private void OnSave(object parameter)
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                var game = new Game
                {
                    Id = Id,
                    Title = Title,
                    Genre = SelectedGenre,
                    Difficulty = SelectedDifficulty,
                    MinPlayers = MinPlayers,
                    MaxPlayers = MaxPlayers,
                    PlayTime = PlayTime,
                    Publisher = Publisher,
                    YearPublished = YearPublished,
                    BggRating = BggRating,
                    PersonalRating = PersonalRating,
                    Status = SelectedStatus
                };

                if (_gameId.HasValue)
                {
                    // Обновление существующей игры
                    _gameService.UpdateGame(game);

                    // Обновление тегов игры
                    _gameService.UpdateGameTags(game.Id, SelectedTags.Select(t => t.Id).ToList());
                }
                else
                {
                    // Добавление новой игры
                    var addedGameId = _gameService.AddGame(game);

                    // Добавление тегов для новой игры
                    _gameService.UpdateGameTags(addedGameId, SelectedTags.Select(t => t.Id).ToList());
                }

                // Закрытие окна
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при сохранении игры: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnCancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}