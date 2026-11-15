using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TechStore1.Authorization;
using TechStore1.Models;
using TechStore1.Services;

namespace TechStore1.Controllers;

[Authorize(Roles = AppRoles.AdministradorOVendedor)]
public class ReportesController : Controller
{
    private readonly IProductoService _productos;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(IProductoService productos, ILogger<ReportesController> logger)
    {
        _productos = productos;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Pdf()
    {
        try
        {
            return await GenerarPdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el reporte PDF de productos");
            TempData["Error"] = "No se pudo generar el reporte PDF. Intente nuevamente.";
            return RedirectToAction("Index", "Productos");
        }
    }

    private async Task<IActionResult> GenerarPdf()
    {
        var productos = await _productos.ObtenerTodosAsync();
        QuestPDF.Settings.License = LicenseType.Community; // Verifique condiciones de la licencia según su uso.

        var documento = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(t => t.FontSize(9));
                page.Header().Column(col =>
                {
                    col.Item().Text("TechStore").FontSize(19).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Reporte de productos").FontSize(13).SemiBold();
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}    |    Total: {productos.Count} productos")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingBottom(12);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2);   // Nombre
                        c.RelativeColumn(3);   // Descripción
                        c.RelativeColumn(1);   // Precio
                        c.RelativeColumn(2);   // Categoría
                        c.RelativeColumn(1);   // Stock
                        c.RelativeColumn(1);   // Estado
                    });
                    table.Header(header =>
                    {
                        foreach (var titulo in new[] { "Nombre", "Descripción", "Precio", "Categoría", "Stock", "Estado" })
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(6)
                                .Text(titulo).FontColor(Colors.White).Bold();
                        }
                    });
                    foreach (var producto in productos)
                    {
                        void Celda(string valor) => table.Cell().BorderBottom(0.5f)
                            .BorderColor(Colors.Grey.Lighten2).Padding(5).Text(valor);
                        Celda(producto.Nombre);
                        Celda(producto.Descripcion);
                        Celda($"$ {producto.Precio:N2}");
                        Celda(producto.Categoria?.Nombre ?? "Sin categoría");
                        Celda(producto.Stock.ToString());
                        Celda(producto.Estado ? "Activo" : "Inactivo");
                    }
                });
                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        });

        var bytes = documento.GeneratePdf();
        _logger.LogInformation("Reporte PDF generado por {Usuario} con {Cantidad} productos", User.Identity?.Name, productos.Count);
        return File(bytes, "application/pdf", $"TechStore_Productos_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Excel()
    {
        try
        {
            return await GenerarExcel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar los productos a Excel");
            TempData["Error"] = "No se pudo generar el archivo Excel. Intente nuevamente.";
            return RedirectToAction("Index", "Productos");
        }
    }

    private async Task<IActionResult> GenerarExcel()
    {
        var productos = await _productos.ObtenerTodosAsync();
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Productos");
        hoja.Cell("A1").Value = "TECHSTORE - REPORTE DE PRODUCTOS";
        hoja.Range("A1:G1").Merge();
        hoja.Range("A1:G1").Style.Font.Bold = true;
        hoja.Range("A1:G1").Style.Font.FontSize = 16;
        hoja.Range("A1:G1").Style.Font.FontColor = XLColor.White;
        hoja.Range("A1:G1").Style.Fill.BackgroundColor = XLColor.DarkBlue;
        hoja.Cell("A2").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        var encabezados = new[] { "ID", "Nombre", "Descripción", "Precio", "Categoría", "Stock", "Estado" };
        for (var col = 1; col <= encabezados.Length; col++)
            hoja.Cell(4, col).Value = encabezados[col - 1];

        var fila = 5;
        foreach (var producto in productos)
        {
            hoja.Cell(fila, 1).Value = producto.Id;
            hoja.Cell(fila, 2).Value = producto.Nombre;
            hoja.Cell(fila, 3).Value = producto.Descripcion;
            hoja.Cell(fila, 4).Value = producto.Precio;
            hoja.Cell(fila, 5).Value = producto.Categoria?.Nombre ?? "Sin categoría";
            hoja.Cell(fila, 6).Value = producto.Stock;
            hoja.Cell(fila, 7).Value = producto.Estado ? "Activo" : "Inactivo";
            fila++;
        }

        var ultimaFila = Math.Max(4, fila - 1);
        var rango = hoja.Range(4, 1, ultimaFila, 7);
        var tabla = rango.CreateTable();
        tabla.Theme = XLTableTheme.TableStyleMedium2;
        hoja.Column(4).Style.NumberFormat.Format = "\"$\"#,##0.00";
        hoja.Columns(1, 7).AdjustToContents();
        hoja.Column(3).Width = Math.Min(65, hoja.Column(3).Width);
        hoja.SheetView.FreezeRows(4);
        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        _logger.LogInformation("Reporte Excel generado por {Usuario} con {Cantidad} productos", User.Identity?.Name, productos.Count);
        return File(memoria.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"TechStore_Productos_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
