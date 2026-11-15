using System.ComponentModel.DataAnnotations;

namespace TechStore1.Models
{
    public class UsuarioRolViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Email { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class CrearUsuarioViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Correo no válido")]
        [Display(Name = "Correo (opcional)")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "La contraseña debe tener al menos 5 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;
    }
}
