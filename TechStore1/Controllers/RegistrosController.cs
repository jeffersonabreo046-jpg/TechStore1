using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStore1.Authorization;
using TechStore1.Services;

namespace TechStore1.Controllers
{
    // Visor de logs (solo Administrador)
    [Authorize(Roles = AppRoles.Administrador)]
    public class RegistrosController : Controller
    {
        private static readonly Regex InicioEvento = new(@"^\d{4}-\d{2}-\d{2} ", RegexOptions.Compiled);

        private readonly LogFileOptions _opciones;
        private readonly ILogger<RegistrosController> _logger;

        public RegistrosController(LogFileOptions opciones, ILogger<RegistrosController> logger)
        {
            _opciones = opciones;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? archivo = null, string? nivel = null, int lineas = 300)
        {
            var archivos = Directory.Exists(_opciones.Carpeta)
                ? Directory.GetFiles(_opciones.Carpeta, "techstore-*.log")
                    .Select(f => Path.GetFileName(f))
                    .OrderByDescending(n => n)
                    .ToList()
                : new List<string>();

            var seleccionado = archivo != null && archivos.Contains(archivo) ? archivo : archivos.FirstOrDefault();
            var contenido = new List<string>();

            if (seleccionado != null)
            {
                // El archivo sigue abierto por Serilog
                var ruta = Path.Combine(_opciones.Carpeta, seleccionado);
                await using var stream = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var lector = new StreamReader(stream);
                var lineasArchivo = (await lector.ReadToEndAsync()).ReplaceLineEndings("\n")
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries);

                // Agrupa cada evento con su stack trace
                var eventos = new List<string>();
                foreach (var linea in lineasArchivo)
                {
                    if (eventos.Count == 0 || InicioEvento.IsMatch(linea))
                        eventos.Add(linea);
                    else
                        eventos[^1] += "\n" + linea;
                }

                IEnumerable<string> filtrados = eventos;
                if (!string.IsNullOrEmpty(nivel))
                {
                    filtrados = filtrados.Where(e => e.Contains($"[{nivel}]"));
                }

                contenido = filtrados.TakeLast(Math.Clamp(lineas, 10, 2000)).Reverse().ToList();
            }

            ViewBag.Archivos = archivos;
            ViewBag.Seleccionado = seleccionado;
            ViewBag.Nivel = nivel;
            ViewBag.Carpeta = _opciones.Carpeta;
            return View(contenido);
        }

        // Genera un error de prueba
        public IActionResult ProbarError()
        {
            _logger.LogWarning("El administrador {Usuario} solicitó una prueba de error", User.Identity?.Name);
            throw new InvalidOperationException("Excepción de prueba generada para demostrar el manejo de errores.");
        }
    }
}
