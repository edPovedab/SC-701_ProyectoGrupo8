using System.ComponentModel.DataAnnotations;

namespace PW.News8.Shared.Models
{
    public class AppSetting
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Clave")]
        public string Key { get; set; }

        [Required]
        [Display(Name = "Valor")]
        public string Value { get; set; }

        [MaxLength(300)]
        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Display(Name = "Es Secret")]
        public bool IsSecret { get; set; } = false;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}