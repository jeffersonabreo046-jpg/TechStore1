using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using TechStore1.Models;

namespace TechStore1.Controllers
{
    public class HomeController : Controller
    {
        private readonly List<Categoria> _categorias = new()
        {
            new Categoria { Id = 1, Nombre = "Laptops", Descripcion = "Computadoras portátiles", Icono = "bi bi-laptop" },
            new Categoria { Id = 2, Nombre = "Smartphones", Descripcion = "Teléfonos inteligentes", Icono = "bi bi-phone" },
            new Categoria { Id = 3, Nombre = "Accesorios", Descripcion = "Periféricos y más", Icono = "bi bi-headset" },
            new Categoria { Id = 4, Nombre = "Monitores", Descripcion = "Pantallas de alta resolución", Icono = "bi bi-display" }
        };

        private readonly List<Producto> _productos = new()
        {
            new Producto { Id = 1, Nombre = "Laptop Pro X", Descripcion = "Core i7, 16GB RAM, 512GB SSD", Precio = 1200.00m, Categoria = "Laptops", Imagen = "novabook-pro-14.jpg", Stock = 10, Estado = true },
            new Producto { Id = 2, Nombre = "Smartphone G", Descripcion = "Pantalla OLED, 128GB, 5G", Precio = 800.00m, Categoria = "Smartphones", Imagen = "pixelwave-x1.jpg", Stock = 20, Estado = true },
            new Producto { Id = 3, Nombre = "Auriculares Inalámbricos", Descripcion = "Cancelación de ruido act.", Precio = 150.00m, Categoria = "Accesorios", Imagen = "sonicpulse-anc.jpg", Stock = 50, Estado = true },
            new Producto { Id = 4, Nombre = "Monitor 4K 27", Descripcion = "Monitor IPS para diseño", Precio = 350.00m, Categoria = "Monitores", Imagen = "visiondock-27.jpg", Stock = 15, Estado = true },
            new Producto { Id = 5, Nombre = "Mouse gamer RGB", Descripcion = "LIGHTSYNC RGB personalizable", Precio = 80.00m, Categoria = "Accesorios", Imagen = "gamecore-m5.jpg", Stock = 25, Estado = true },
            new Producto { Id = 6, Nombre = "Teclado Mecánico", Descripcion = "Switches RGB y blue", Precio = 90.00m, Categoria = "Accesorios", Imagen = "hyperstrike-k7.jpg", Stock = 30, Estado = true }
        };

        public IActionResult Index()
        {
            var destacados = _productos.Take(3).ToList();
            return View(destacados);
        }

        public IActionResult Productos()
        {
            ViewBag.Categorias = _categorias;
            return View(_productos);
        }

        public IActionResult Categorias()
        {
            return View(_categorias);
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
