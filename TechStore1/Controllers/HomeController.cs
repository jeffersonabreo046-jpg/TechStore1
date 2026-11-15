using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechStore1.Models;
using TechStore1.Services;

namespace TechStore1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IProductoService productoService, ICategoriaService categoriaService,
            ILogger<HomeController> logger)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            var destacados = productos.Where(p => p.Estado).Take(3).ToList();
            return View(destacados);
        }

        // Catálogo: cualquier usuario autenticado
        [Authorize]
        public async Task<IActionResult> Productos()
        {
            ViewBag.Categorias = await _categoriaService.ObtenerTodasAsync();
            var productos = await _productoService.ObtenerTodosAsync();
            return View(productos);
        }

        [Authorize]
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [Authorize]
        public async Task<IActionResult> Categorias()
        {
            var categorias = await _categoriaService.ObtenerTodasAsync();
            return View(categorias);
        }

        public IActionResult Contactenos()
        {
            return View();
        }

        public IActionResult AcercaDeNosotros()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Página de error
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (feature?.Error != null)
            {
                _logger.LogError(feature.Error, "Excepción no controlada en {Ruta}. RequestId: {RequestId}",
                    feature.Path, requestId);
            }

            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View(new ErrorViewModel
            {
                RequestId = requestId,
                CodigoEstado = 500,
                Ruta = feature?.Path
            });
        }

        // Páginas de error por código de estado (404, 403)
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Estado(int id)
        {
            var reExecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            _logger.LogWarning("Código de estado {Codigo} en la ruta {Ruta}", id, reExecute?.OriginalPath);

            Response.StatusCode = id;
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                CodigoEstado = id,
                Ruta = reExecute?.OriginalPath
            });
        }
    }
}
