using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechStore1.Models;

namespace TechStore1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Productos()
        {
            var categorias = new List<Categoria>
            {
                new Categoria { Id = 1, Nombre = "Laptops", Descripcion = "Computadoras portátiles", Icono = "bi bi-laptop" },
                new Categoria { Id = 2, Nombre = "Smartphones", Descripcion = "Teléfonos inteligentes", Icono = "bi bi-phone" },
                new Categoria { Id = 3, Nombre = "Accesorios", Descripcion = "Periféricos y más", Icono = "bi bi-headset" },
                new Categoria { Id = 4, Nombre = "Monitores", Descripcion = "Pantallas de alta resolución", Icono = "bi bi-display" }
            };

            var productos = new List<Producto>
            {
                new Producto { Id = 1, Nombre = "Laptop Pro X", Descripcion = "Core i7, 16GB RAM, 512GB SSD", Precio = 1200.00m, Categoria = "Laptops", Imagen = "laptop.png", Stock = 10, Estado = true },
                new Producto { Id = 2, Nombre = "Smartphone Z", Descripcion = "Pantalla OLED, 128GB, 5G", Precio = 800.00m, Categoria = "Smartphones", Imagen = "smartphone.png", Stock = 20, Estado = true },
                new Producto { Id = 3, Nombre = "Auriculares Inalámbricos", Descripcion = "Cancelación de ruido act.", Precio = 150.00m, Categoria = "Accesorios", Imagen = "headphones.png", Stock = 50, Estado = true },
                new Producto { Id = 4, Nombre = "Monitor 4K 27", Descripcion = "Monitor IPS para diseño", Precio = 350.00m, Categoria = "Monitores", Imagen = "monitor.png", Stock = 15, Estado = true },
                new Producto { Id = 5, Nombre = "Laptop Lite", Descripcion = "Core i5, 8GB RAM, 256GB SSD", Precio = 800.00m, Categoria = "Laptops", Imagen = "laptop-lite.png", Stock = 25, Estado = true },
                new Producto { Id = 6, Nombre = "Teclado Mecánico", Descripcion = "Switches RGB y blue", Precio = 90.00m, Categoria = "Accesorios", Imagen = "keyboard.png", Stock = 30, Estado = true }
            };

            ViewBag.Categorias = categorias;
            return View(productos);
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
