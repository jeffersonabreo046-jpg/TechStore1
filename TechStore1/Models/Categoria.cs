using System.ComponentModel.DataAnnotations;

namespace TechStore1.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        public string Icono { get; set; } = string.Empty;

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}