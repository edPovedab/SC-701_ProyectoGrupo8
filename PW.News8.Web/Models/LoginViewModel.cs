using System.ComponentModel.DataAnnotations;

namespace PW.News8.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        // Se usa para volver a la página original si el usuario
        // fue redirigido al login por un [Authorize] (ReturnUrl).
        public string? ReturnUrl { get; set; }
    }
}