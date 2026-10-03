namespace TechStore1.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace TechStore1.Models
    {
        public class Producto
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(100)]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "La descripcion es obligatoria")]
            [StringLength(500)]
            public string Descripcion { get; set; } = string.Empty;

            [Required(ErrorMessage = "El precio es obligatorio")]
            [Range(0.01, 999999.99, ErrorMessage = "Ingrese un precio valido")]
            [Column(TypeName = "decimal(18,2)")]
            public decimal Precio { get; set; }

            public string Imagen { get; set; } = string.Empty;

            [Required(ErrorMessage = "El stock es obligatorio")]
            [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
            public int Stock { get; set; }

            public bool Estado { get; set; } = true;

            [Required(ErrorMessage = "Debe seleccionar una categoria")]
            public int CategoriaId { get; set; }

            public Categoria? Categoria { get; set; }
        }
    }
}