using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoardGameCollection.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<GameTag> GameTags { get; set; } = new List<GameTag>();
    }

    public class GameTag
    {
        public int GameId { get; set; }
        public virtual Game Game { get; set; }

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}