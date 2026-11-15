using Microsoft.EntityFrameworkCore;
using TechStore1.Data;
using TechStore1.Models;

namespace TechStore1.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(ApplicationDbContext context, ILogger<ProductoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                _logger.LogInformation("Se consultaron {Cantidad} productos", productos.Count);
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar la lista de productos");
                throw;
            }
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Include(p => p.Categoria)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                {
                    _logger.LogWarning("Producto con Id {ProductoId} no encontrado", id);
                }

                return producto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el producto con Id {ProductoId}", id);
                throw;
            }
        }

        public async Task AgregarAsync(Producto producto)
        {
            ArgumentNullException.ThrowIfNull(producto);

            try
            {
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto registrado: {Nombre} (Id {ProductoId})", producto.Nombre, producto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el producto {Nombre}", producto.Nombre);
                throw;
            }
        }

        public async Task<bool> EditarAsync(Producto producto)
        {
            ArgumentNullException.ThrowIfNull(producto);

            try
            {
                var productoExistente = await _context.Productos.FindAsync(producto.Id);
                if (productoExistente == null)
                {
                    _logger.LogWarning("No se pudo editar: producto con Id {ProductoId} no existe", producto.Id);
                    return false;
                }

                productoExistente.Nombre = producto.Nombre;
                productoExistente.Descripcion = producto.Descripcion;
                productoExistente.Precio = producto.Precio;
                productoExistente.Imagen = producto.Imagen;
                productoExistente.Stock = producto.Stock;
                productoExistente.Estado = producto.Estado;
                productoExistente.CategoriaId = producto.CategoriaId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto editado: {Nombre} (Id {ProductoId})", producto.Nombre, producto.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar el producto con Id {ProductoId}", producto.Id);
                throw;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning("No se pudo eliminar: producto con Id {ProductoId} no existe", id);
                    return false;
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto eliminado: {Nombre} (Id {ProductoId})", producto.Nombre, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto con Id {ProductoId}", id);
                throw;
            }
        }
    }
}
