using BoardGameCollection.Data;
using BoardGameCollection.Services;
using BoardGameCollection.ViewModels;
using BoardGameCollection.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace BoardGameCollection
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        private bool _databaseInitialized = false;

        public App()
        {
            // Настройка глобального обработчика исключений
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Корректный режим завершения приложения
            ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                _serviceProvider = serviceCollection.BuildServiceProvider();

                InitializeDatabase();
                ShowMainWindow();
            }
            catch (Exception ex)
            {
                ShowErrorDialog("Критическая ошибка запуска приложения", ex);
                Shutdown();
            }

            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BoardGameContext>(options =>
                options.UseSqlite("Data Source=boardgames.db"));

            services.AddSingleton<IGameService, GameService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<GameViewModel>();
            services.AddTransient<SessionViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<GameWindow>();
            services.AddTransient<SessionWindow>();
        }

        private void InitializeDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BoardGameContext>();

            try
            {
                Debug.WriteLine("Инициализация базы данных");

                // Пересоздаем базу данных для гарантированного наличия тестовых данных
                if (context.DatabaseExists())
                {
                    Debug.WriteLine("База данных существует, удаляем для пересоздания");
                    context.Database.EnsureDeleted();
                }

                Debug.WriteLine("Создание новой базы данных");
                context.Database.EnsureCreated();

                // Заполнение данными
                SeedDatabase(context);

                _databaseInitialized = true;
                Debug.WriteLine("База данных успешно инициализирована");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ShowErrorDialog("Ошибка инициализации базы данных", ex);
                throw;
            }
        }

        private void SeedDatabase(BoardGameContext context)
        {
            try
            {
                Debug.WriteLine("Заполнение базы данных тестовыми данными");

                // Создание тегов
                var strategyTag = new Models.Tag { Name = "Стратегия" };
                var partyTag = new Models.Tag { Name = "Вечеринка" };
                var kidsTag = new Models.Tag { Name = "Для детей" };
                var shortGamesTag = new Models.Tag { Name = "Короткие" };
                var detectiveTag = new Models.Tag { Name = "Детектив" };
                var economicTag = new Models.Tag { Name = "Экономическая" };
                var cooperativeTag = new Models.Tag { Name = "Кооперативная" };
                var cardGameTag = new Models.Tag { Name = "Карточная" };
                var familyTag = new Models.Tag { Name = "Семейная" };

                context.Tags.AddRange(strategyTag, partyTag, kidsTag, shortGamesTag,
                                     detectiveTag, economicTag, cooperativeTag, cardGameTag, familyTag);
                context.SaveChanges();
                Debug.WriteLine($"Создано тегов: {context.Tags.Count()}");

                // Создание большого количества тестовых игр
                var games = new List<Models.Game>
                {
                    new Models.Game
                    {
                        Title = "Катан",
                        Genre = Models.Genre.Strategy,
                        Difficulty = Models.Difficulty.Medium,
                        MinPlayers = 3,
                        MaxPlayers = 4,
                        PlayTime = 90,
                        Publisher = "KOSMOS",
                        YearPublished = 1995,
                        BggRating = 8.3,
                        PersonalRating = 9,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-30)
                    },
                    new Models.Game
                    {
                        Title = "Монополия",
                        Genre = Models.Genre.Economic,
                        Difficulty = Models.Difficulty.Easy,
                        MinPlayers = 2,
                        MaxPlayers = 8,
                        PlayTime = 180,
                        Publisher = "Hasbro",
                        YearPublished = 1935,
                        BggRating = 4.5,
                        PersonalRating = 6,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-45)
                    },
                    new Models.Game
                    {
                        Title = "Каркассон",
                        Genre = Models.Genre.Strategy,
                        Difficulty = Models.Difficulty.Easy,
                        MinPlayers = 2,
                        MaxPlayers = 5,
                        PlayTime = 45,
                        Publisher = "Hans im Glück",
                        YearPublished = 2000,
                        BggRating = 7.4,
                        PersonalRating = 8,
                        Status = Models.GameStatus.WantToBuy,
                        DateAdded = DateTime.Now.AddDays(-10)
                    },
                    new Models.Game
                    {
                        Title = "7 чудес",
                        Genre = Models.Genre.Strategy,
                        Difficulty = Models.Difficulty.Medium,
                        MinPlayers = 3,
                        MaxPlayers = 7,
                        PlayTime = 30,
                        Publisher = "Repos Production",
                        YearPublished = 2010,
                        BggRating = 7.9,
                        PersonalRating = 9,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-20)
                    },
                    new Models.Game
                    {
                        Title = "Клаустрафобия",
                        Genre = Models.Genre.Cooperative,
                        Difficulty = Models.Difficulty.Medium,
                        MinPlayers = 2,
                        MaxPlayers = 6,
                        PlayTime = 60,
                        Publisher = "Edge Entertainment",
                        YearPublished = 2008,
                        BggRating = 7.3,
                        PersonalRating = 8,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-25)
                    },
                    new Models.Game
                    {
                        Title = "Тайм стори",
                        Genre = Models.Genre.Detective,
                        Difficulty = Models.Difficulty.Hard,
                        MinPlayers = 2,
                        MaxPlayers = 5,
                        PlayTime = 90,
                        Publisher = "Space Cowboys",
                        YearPublished = 2013,
                        BggRating = 7.8,
                        PersonalRating = 10,
                        Status = Models.GameStatus.WantToBuy,
                        DateAdded = DateTime.Now.AddDays(-5)
                    },
                    new Models.Game
                    {
                        Title = "Свинтус",
                        Genre = Models.Genre.Card,
                        Difficulty = Models.Difficulty.Easy,
                        MinPlayers = 2,
                        MaxPlayers = 8,
                        PlayTime = 15,
                        Publisher = "Gaga Games",
                        YearPublished = 2009,
                        BggRating = 6.5,
                        PersonalRating = 7,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-15)
                    },
                    new Models.Game
                    {
                        Title = "Декстерити",
                        Genre = Models.Genre.Family,
                        Difficulty = Models.Difficulty.Easy,
                        MinPlayers = 2,
                        MaxPlayers = 4,
                        PlayTime = 30,
                        Publisher = "Мосигра",
                        YearPublished = 2016,
                        BggRating = 7.1,
                        PersonalRating = 8,
                        Status = Models.GameStatus.ForSale,
                        DateAdded = DateTime.Now.AddDays(-7)
                    },
                    new Models.Game
                    {
                        Title = "Городской квест",
                        Genre = Models.Genre.Family,
                        Difficulty = Models.Difficulty.Easy,
                        MinPlayers = 3,
                        MaxPlayers = 6,
                        PlayTime = 45,
                        Publisher = "Простые правила",
                        YearPublished = 2018,
                        BggRating = 6.8,
                        PersonalRating = 7,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-12)
                    },
                    new Models.Game
                    {
                        Title = "Экспансия",
                        Genre = Models.Genre.Economic,
                        Difficulty = Models.Difficulty.Hard,
                        MinPlayers = 2,
                        MaxPlayers = 4,
                        PlayTime = 120,
                        Publisher = "Feuerland Spiele",
                        YearPublished = 2015,
                        BggRating = 8.2,
                        PersonalRating = 9,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-18)
                    },
                    new Models.Game
                    {
                        Title = "Город монстров",
                        Genre = Models.Genre.Family,
                        Difficulty = Models.Difficulty.Easy,
                        MinPlayers = 2,
                        MaxPlayers = 6,
                        PlayTime = 30,
                        Publisher = "Город игр",
                        YearPublished = 2012,
                        BggRating = 6.9,
                        PersonalRating = 7,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-22)
                    },
                    new Models.Game
                    {
                        Title = "Шакал",
                        Genre = Models.Genre.Strategy,
                        Difficulty = Models.Difficulty.Medium,
                        MinPlayers = 2,
                        MaxPlayers = 4,
                        PlayTime = 60,
                        Publisher = "Мосигра",
                        YearPublished = 2007,
                        BggRating = 7.5,
                        PersonalRating = 9,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-28)
                    },
                    new Models.Game
                    {
                        Title = "Манчкин",
                        Genre = Models.Genre.Card,
                        Difficulty = Models.Difficulty.Easy,
                        MinPlayers = 3,
                        MaxPlayers = 6,
                        PlayTime = 90,
                        Publisher = "Игромания",
                        YearPublished = 2001,
                        BggRating = 7.2,
                        PersonalRating = 8,
                        Status = Models.GameStatus.InCollection,
                        DateAdded = DateTime.Now.AddDays(-35)
                    },
                    new Models.Game
                    {
                        Title = "Дикари",
                        Genre = Models.Genre.Strategy,
                        Difficulty = Models.Difficulty.Medium,
                        MinPlayers = 2,
                        MaxPlayers = 6,
                        PlayTime = 75,
                        Publisher = "Москва-Сити",
                        YearPublished = 2016,
                        BggRating = 7.6,
                        PersonalRating = 8,
                        Status = Models.GameStatus.WantToBuy,
                        DateAdded = DateTime.Now.AddDays(-3)
                    },
                    new Models.Game
                    {
                        Title = "Городской квест: Хроники Нью-Йорка",
                        Genre = Models.Genre.Detective,
                        Difficulty = Models.Difficulty.Hard,
                        MinPlayers = 1,
                        MaxPlayers = 6,
                        PlayTime = 150,
                        Publisher = "Правильные игры",
                        YearPublished = 2019,
                        BggRating = 8.1,
                        PersonalRating = 10,
                        Status = Models.GameStatus.WantToBuy,
                        DateAdded = DateTime.Now.AddDays(-1)
                    }
                };

                context.Games.AddRange(games);
                context.SaveChanges();
                Debug.WriteLine($"Создано игр: {context.Games.Count()}");

                // Добавление игровых сессий
                var sessions = new List<Models.GameSession>
                {
                    new Models.GameSession
                    {
                        GameId = games[0].Id, // Катан
                        SessionDate = DateTime.Now.AddDays(-5),
                        PlayersCount = 4,
                        Results = "Победил Алексей",
                        SessionRating = 9
                    },
                    new Models.GameSession
                    {
                        GameId = games[0].Id, // Катан
                        SessionDate = DateTime.Now.AddDays(-10),
                        PlayersCount = 3,
                        Results = "Победила Мария",
                        SessionRating = 8
                    },
                    new Models.GameSession
                    {
                        GameId = games[1].Id, // Монополия
                        SessionDate = DateTime.Now.AddDays(-15),
                        PlayersCount = 4,
                        Results = "Победил Иван",
                        SessionRating = 6
                    },
                    new Models.GameSession
                    {
                        GameId = games[3].Id, // 7 чудес
                        SessionDate = DateTime.Now.AddDays(-8),
                        PlayersCount = 5,
                        Results = "Победил Петр",
                        SessionRating = 9
                    },
                    new Models.GameSession
                    {
                        GameId = games[4].Id, // Клаустрафобия
                        SessionDate = DateTime.Now.AddDays(-2),
                        PlayersCount = 4,
                        Results = "Победили монстры",
                        SessionRating = 10
                    },
                    new Models.GameSession
                    {
                        GameId = games[6].Id, // Свинтус
                        SessionDate = DateTime.Now.AddDays(-3),
                        PlayersCount = 5,
                        Results = "Самый ловкий - Сергей",
                        SessionRating = 8
                    },
                    new Models.GameSession
                    {
                        GameId = games[8].Id, // Городской квест
                        SessionDate = DateTime.Now.AddDays(-20),
                        PlayersCount = 4,
                        Results = "Завершено за 45 минут",
                        SessionRating = 7
                    },
                    new Models.GameSession
                    {
                        GameId = games[11].Id, // Шакал
                        SessionDate = DateTime.Now.AddDays(-4),
                        PlayersCount = 3,
                        Results = "Приключения пиратов",
                        SessionRating = 9
                    },
                    new Models.GameSession
                    {
                        GameId = games[12].Id, // Манчкин
                        SessionDate = DateTime.Now.AddDays(-12),
                        PlayersCount = 5,
                        Results = "Уровень 10 достигнут",
                        SessionRating = 8
                    }
                };

                context.GameSessions.AddRange(sessions);
                context.SaveChanges();
                Debug.WriteLine($"Создано игровых сессий: {context.GameSessions.Count()}");

                // Связь игр с тегами
                var gameTags = new List<Models.GameTag>
                {
                    // Катан
                    new Models.GameTag { GameId = games[0].Id, TagId = strategyTag.Id },
                    new Models.GameTag { GameId = games[0].Id, TagId = partyTag.Id },
                    // Монополия
                    new Models.GameTag { GameId = games[1].Id, TagId = economicTag.Id },
                    new Models.GameTag { GameId = games[1].Id, TagId = familyTag.Id },
                    // Каркассон
                    new Models.GameTag { GameId = games[2].Id, TagId = strategyTag.Id },
                    new Models.GameTag { GameId = games[2].Id, TagId = shortGamesTag.Id },
                    // 7 чудес
                    new Models.GameTag { GameId = games[3].Id, TagId = strategyTag.Id },
                    new Models.GameTag { GameId = games[3].Id, TagId = economicTag.Id },
                    // Клаустрафобия
                    new Models.GameTag { GameId = games[4].Id, TagId = cooperativeTag.Id },
                    new Models.GameTag { GameId = games[4].Id, TagId = detectiveTag.Id },
                    // Тайм стори
                    new Models.GameTag { GameId = games[5].Id, TagId = detectiveTag.Id },
                    new Models.GameTag { GameId = games[5].Id, TagId = cooperativeTag.Id },
                    // Свинтус
                    new Models.GameTag { GameId = games[6].Id, TagId = cardGameTag.Id },
                    new Models.GameTag { GameId = games[6].Id, TagId = partyTag.Id },
                    // Декстерити
                    new Models.GameTag { GameId = games[7].Id, TagId = familyTag.Id },
                    new Models.GameTag { GameId = games[7].Id, TagId = shortGamesTag.Id },
                    // Городской квест
                    new Models.GameTag { GameId = games[8].Id, TagId = familyTag.Id },
                    new Models.GameTag { GameId = games[8].Id, TagId = detectiveTag.Id },
                    // Экспансия
                    new Models.GameTag { GameId = games[9].Id, TagId = economicTag.Id },
                    new Models.GameTag { GameId = games[9].Id, TagId = strategyTag.Id },
                    // Город монстров
                    new Models.GameTag { GameId = games[10].Id, TagId = familyTag.Id },
                    new Models.GameTag { GameId = games[10].Id, TagId = partyTag.Id },
                    // Шакал
                    new Models.GameTag { GameId = games[11].Id, TagId = strategyTag.Id },
                    new Models.GameTag { GameId = games[11].Id, TagId = familyTag.Id },
                    // Манчкин
                    new Models.GameTag { GameId = games[12].Id, TagId = cardGameTag.Id },
                    new Models.GameTag { GameId = games[12].Id, TagId = partyTag.Id },
                    // Дикари
                    new Models.GameTag { GameId = games[13].Id, TagId = strategyTag.Id },
                    new Models.GameTag { GameId = games[13].Id, TagId = economicTag.Id },
                    // Городской квест: Хроники Нью-Йорка
                    new Models.GameTag { GameId = games[14].Id, TagId = detectiveTag.Id },
                    new Models.GameTag { GameId = games[14].Id, TagId = familyTag.Id }
                };

                context.GameTags.AddRange(gameTags);
                context.SaveChanges();
                Debug.WriteLine($"Создано связей игры-теги: {context.GameTags.Count()}");

                // Проверка заполненности данных
                Debug.WriteLine($"Проверка данных:");
                Debug.WriteLine($"Всего игр в БД: {context.Games.Count()}");
                Debug.WriteLine($"Всего тегов в БД: {context.Tags.Count()}");
                Debug.WriteLine($"Всего игровых сессий в БД: {context.GameSessions.Count()}");
                Debug.WriteLine($"Всего связей игры-теги в БД: {context.GameTags.Count()}");

                _databaseInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка заполнения базы данных: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ShowErrorDialog("Ошибка заполнения тестовыми данными", ex);
            }
        }

        private void ShowMainWindow()
        {
            try
            {
                Debug.WriteLine("Показ главного окна");
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

                mainWindow.DataContext = mainViewModel;
                mainWindow.Closed += MainWindow_Closed;

                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отображения главного окна: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                ShowErrorDialog("Ошибка отображения главного окна", ex);
                throw;
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Явное завершение приложения при закрытии главного окна
            Shutdown();
        }

        private void ShowErrorDialog(string title, Exception ex)
        {
            string message = $"Ошибка: {ex.Message}\n\n{ex.StackTrace}";

            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorDialog("Необработанное исключение в UI потоке", e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowErrorDialog("Необработанное исключение в приложении", ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Освобождение ресурсов
            (_serviceProvider as IDisposable)?.Dispose();
            base.OnExit(e);
        }
    }
}