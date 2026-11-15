using Microsoft.AspNetCore.Mvc;
using TechStore1.Services;

namespace TechStore1.ViewComponents
{
    // Resumen del inventario
    public class ResumenInventarioViewComponent : ViewComponent
    {
        private readonly IProductoService _productoService;

        public ResumenInventarioViewComponent(IProductoService productoService)
        {
            _productoService = productoService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int umbralStockBajo = 10)
        {
            var productos = await _productoService.ObtenerTodosAsync();

            var modelo = new ResumenInventarioModel
            {
                TotalProductos = productos.Count,
                ProductosActivos = productos.Count(p => p.Estado),
                UnidadesEnStock = productos.Sum(p => p.Stock),
                ValorInventario = productos.Sum(p => p.Precio * p.Stock),
                UmbralStockBajo = umbralStockBajo,
                ProductosStockBajo = productos
                    .Where(p => p.Stock <= umbralStockBajo)
                    .Select(p => p.Nombre)
                    .ToList()
            };

            return View(modelo);
        }
    }

    public class ResumenInventarioModel
    {
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
        public int UnidadesEnStock { get; set; }
        public decimal ValorInventario { get; set; }
        public int UmbralStockBajo { get; set; }
        public List<string> ProductosStockBajo { get; set; } = new();
    }
}
