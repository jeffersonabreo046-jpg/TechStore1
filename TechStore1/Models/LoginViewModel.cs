using System.ComponentModel.DataAnnotations;

namespace TechStore1.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "La contraseña debe tener al menos 5 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
