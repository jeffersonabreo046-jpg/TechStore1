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

        public HomeController(IProductoService productoService, ICategoriaService categoriaService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            var destacados = productos.Take(3).ToList();
            return View(destacados);
        }

        public async Task<IActionResult> Productos()
        {
            ViewBag.Categorias = await _categoriaService.ObtenerTodasAsync();
            var productos = await _productoService.ObtenerTodosAsync();
            return View(productos);
        }

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
