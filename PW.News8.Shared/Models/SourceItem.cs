using System.ComponentModel.DataAnnotations;

namespace PW.News8.Shared.Models
{
    public class SourceItem
    {
        public int Id { get; set; }

        public int SourceId { get; set; }

        [Required]
        // JSON completo del item de noticias
        public string Json { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navegación
        public Source Source { get; set; }
    }
}