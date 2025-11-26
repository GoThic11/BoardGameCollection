using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameCollection.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название игры обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Title { get; set; }

        public Genre Genre { get; set; }

        public Difficulty Difficulty { get; set; }

        [Range(1, 20, ErrorMessage = "Минимальное количество игроков должно быть от 1 до 20")]
        public int MinPlayers { get; set; }

        [Range(1, 20, ErrorMessage = "Максимальное количество игроков должно быть от 1 до 20")]
        public int MaxPlayers { get; set; }

        [Range(5, 600, ErrorMessage = "Время игры должно быть от 5 до 600 минут")]
        public int PlayTime { get; set; }

        public string Publisher { get; set; }

        [Range(1900, 2100, ErrorMessage = "Год издания должен быть в диапазоне 1900-2100")]
        public int YearPublished { get; set; }

        [Range(1, 10, ErrorMessage = "Рейтинг BGG должен быть от 1 до 10")]
        public double? BggRating { get; set; }

        [Range(1, 10, ErrorMessage = "Личный рейтинг должен быть от 1 до 10")]
        public int PersonalRating { get; set; }

        public GameStatus Status { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public DateTime? LastPlayed { get; set; }

        // Связь один-ко-многим с игровыми сессиями
        public virtual ICollection<GameSession> Sessions { get; set; } = new List<GameSession>();

        // Связь многие-ко-многим с тегами
        public virtual ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();

        // Свойство только для чтения для определения, была ли игра проверена (есть ли игровые сессии)
        [NotMapped]
        public bool IsUnplayed => Sessions == null || Sessions.Count == 0;

        // Свойство только для чтения для стоимости одного игрового вечера (условный расчет)
        [NotMapped]
        public double CostPerSession => Sessions?.Count > 0 ? 500.0 / Sessions.Count : 0;
    }
}