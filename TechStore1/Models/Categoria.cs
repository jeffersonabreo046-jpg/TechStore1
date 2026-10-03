using System.ComponentModel.DataAnnotations;
using TechStore1.Models.TechStore1.Models;

namespace TechStore1.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripcion es obligatoria")]
        [StringLength(250)]
        public string Descripcion { get; set; } = string.Empty;

        public string Icono { get; set; } = string.Empty;
        public ICollection<Producto> Productos { get; set; }
            = new List<Producto>();
    }
}