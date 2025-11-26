using System;
using System.Globalization;
using System.Windows.Data;
using BoardGameCollection.Models;

namespace BoardGameCollection.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return value switch
            {
                Genre genre => GetGenreDescription(genre),
                Difficulty difficulty => GetDifficultyDescription(difficulty),
                GameStatus status => GetStatusDescription(status),
                _ => value.ToString()
            };
        }

        private string GetGenreDescription(Genre genre)
        {
            return genre switch
            {
                Genre.Strategy => "Стратегия",
                Genre.Detective => "Детектив",
                Genre.Cooperative => "Кооперативная",
                Genre.Economic => "Экономическая",
                Genre.Card => "Карточная",
                Genre.Family => "Семейная",
                _ => genre.ToString()
            };
        }

        private string GetDifficultyDescription(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "Простая",
                Difficulty.Medium => "Средняя",
                Difficulty.Hard => "Сложная",
                _ => difficulty.ToString()
            };
        }

        private string GetStatusDescription(GameStatus status)
        {
            return status switch
            {
                GameStatus.InCollection => "В коллекции",
                GameStatus.WantToBuy => "Хочу купить",
                GameStatus.ForSale => "Продается",
                _ => status.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}