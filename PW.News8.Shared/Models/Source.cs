using System.ComponentModel.DataAnnotations;

namespace PW.News8.Shared.Models
{
    public class Source
    {
        public int Id { get; set; }

        [Required, MaxLength(500)]
        [Display(Name = "URL")]
        public string Url { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [MaxLength(500)]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Tipo")]
        // Valores válidos: "json", "xml", "html"
        public string ComponentType { get; set; }

        [Display(Name = "Requiere Secret")]
        public bool RequiresSecret { get; set; } = false;

        // Navegación
        public ICollection<SourceItem> SourceItems { get; set; } = new List<SourceItem>();
    }
}