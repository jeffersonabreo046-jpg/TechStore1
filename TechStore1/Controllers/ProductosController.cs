using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStore1.Authorization;
using TechStore1.Models;
using TechStore1.Services;

namespace TechStore1.Controllers
{
    // Administrador y Vendedor gestionan productos; solo Administrador elimina
    [Authorize(Roles = AppRoles.AdministradorOVendedor)]
    public class ProductosController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IProductoService productoService, ICategoriaService categoriaService,
            ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            return View(productos);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCategorias();
            return View(new Producto { Estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registro de producto con datos inválidos por {Usuario}", User.Identity?.Name);
                await CargarCategorias(producto.CategoriaId);
                return View(producto);
            }

            try
            {
                await _productoService.AgregarAsync(producto);
                _logger.LogInformation("El usuario {Usuario} registró el producto {Nombre}", User.Identity?.Name, producto.Nombre);
                TempData["Mensaje"] = "Producto agregado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el producto {Nombre} por {Usuario}", producto.Nombre, User.Identity?.Name);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar el producto. Intente nuevamente.");
                await CargarCategorias(producto.CategoriaId);
                return View(producto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            await CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                _logger.LogWarning("Edición rechazada: Id de ruta {IdRuta} distinto a Id del producto {IdProducto}", id, producto.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await CargarCategorias(producto.CategoriaId);
                return View(producto);
            }

            try
            {
                var editado = await _productoService.EditarAsync(producto);
                if (!editado)
                {
                    return NotFound();
                }

                _logger.LogInformation("El usuario {Usuario} editó el producto {ProductoId}", User.Identity?.Name, producto.Id);
                TempData["Mensaje"] = "Producto actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar el producto {ProductoId}", producto.Id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el producto. Intente nuevamente.");
                await CargarCategorias(producto.CategoriaId);
                return View(producto);
            }
        }

        [Authorize(Roles = AppRoles.Administrador)]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Administrador)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var eliminado = await _productoService.EliminarAsync(id);
                if (!eliminado)
                {
                    return NotFound();
                }

                _logger.LogInformation("El usuario {Usuario} eliminó el producto {ProductoId}", User.Identity?.Name, id);
                TempData["Mensaje"] = "Producto eliminado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto {ProductoId}", id);
                TempData["Error"] = "No se pudo eliminar el producto. Intente nuevamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategorias(int? seleccionada = null)
        {
            var categorias = await _categoriaService.ObtenerTodasAsync();
            ViewBag.Categorias = new SelectList(categorias, "Id", "Nombre", seleccionada);
        }
    }
}
