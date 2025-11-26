using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameCollection.Models
{
    public class GameSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }

        public DateTime SessionDate { get; set; } = DateTime.Now;

        public int PlayersCount { get; set; }

        public string Results { get; set; }

        [Range(1, 10)]
        public int SessionRating { get; set; }
    }
}