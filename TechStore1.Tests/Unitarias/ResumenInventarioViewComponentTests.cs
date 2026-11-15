using Microsoft.AspNetCore.Mvc.ViewComponents;
using Moq;
using TechStore1.Models;
using TechStore1.Services;
using TechStore1.ViewComponents;

namespace TechStore1.Tests.Unitarias;

[Trait("Categoria", "Unitaria")]
public class ResumenInventarioViewComponentTests
{
    [Fact]
    public async Task InvokeAsync_CalculaTotalesYProductosConStockBajo()
    {
        var servicio = new Mock<IProductoService>();
        servicio.Setup(s => s.ObtenerTodosAsync()).ReturnsAsync(new List<Producto>
        {
            new() { Nombre = "A", Precio = 10m, Stock = 5, Estado = true },
            new() { Nombre = "B", Precio = 20m, Stock = 50, Estado = true },
            new() { Nombre = "C", Precio = 100m, Stock = 0, Estado = false }
        });
        var componente = new ResumenInventarioViewComponent(servicio.Object);

        var resultado = await componente.InvokeAsync(umbralStockBajo: 10);

        var vista = Assert.IsType<ViewViewComponentResult>(resultado);
        var modelo = Assert.IsType<ResumenInventarioModel>(vista.ViewData!.Model);
        Assert.Equal(3, modelo.TotalProductos);
        Assert.Equal(2, modelo.ProductosActivos);
        Assert.Equal(55, modelo.UnidadesEnStock);
        Assert.Equal(1050m, modelo.ValorInventario);
        Assert.Equal(new[] { "A", "C" }, modelo.ProductosStockBajo);
        servicio.Verify(s => s.ObtenerTodosAsync(), Times.Once);
    }
}
