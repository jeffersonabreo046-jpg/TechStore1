using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TechStore1.Data;
using TechStore1.Models;
using TechStore1.Services;
using TechStore1.Tests.Infraestructura;

namespace TechStore1.Tests.Unitarias;

// Pruebas unitarias de ProductoService
[Trait("Categoria", "Unitaria")]
public class ProductoServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<ProductoService>> _loggerMock;
    private readonly ProductoService _servicio;

    public ProductoServiceTests()
    {
        _context = TestDbContextFactory.Crear();
        _loggerMock = new Mock<ILogger<ProductoService>>();
        _servicio = new ProductoService(_context, _loggerMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private static Producto NuevoProducto(string nombre = "Tablet Z10") => new()
    {
        Nombre = nombre,
        Descripcion = "Tablet de 10 pulgadas, 128GB",
        Precio = 299.99m,
        Stock = 12,
        Estado = true,
        CategoriaId = 2,
        Imagen = "tablet.jpg"
    };

    // Consultar productos

    [Fact]
    public async Task ObtenerTodosAsync_RetornaTodosLosProductosConCategoria()
    {
        var productos = await _servicio.ObtenerTodosAsync();

        Assert.NotNull(productos);
        Assert.Equal(6, productos.Count);
        Assert.All(productos, p => Assert.NotNull(p.Categoria));
        _loggerMock.VerificarLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task ObtenerTodosAsync_RetornaProductosOrdenadosPorNombre()
    {
        var productos = await _servicio.ObtenerTodosAsync();

        var nombres = productos.Select(p => p.Nombre).ToList();
        Assert.Equal(nombres.OrderBy(n => n).ToList(), nombres);
        Assert.Equal("Auriculares Inalámbricos", nombres.First());
    }

    [Fact]
    public async Task ObtenerTodosAsync_SinProductos_RetornaListaVacia()
    {
        _context.Productos.RemoveRange(_context.Productos);
        await _context.SaveChangesAsync();

        var productos = await _servicio.ObtenerTodosAsync();

        Assert.Empty(productos);
    }

    // Obtener por ID

    [Theory]
    [InlineData(1, "Laptop Pro X", "Laptops")]
    [InlineData(2, "Smartphone G", "Smartphones")]
    [InlineData(4, "Monitor 4K 27", "Monitores")]
    public async Task ObtenerPorIdAsync_IdExistente_RetornaProductoCorrecto(int id, string nombreEsperado, string categoriaEsperada)
    {
        var producto = await _servicio.ObtenerPorIdAsync(id);

        Assert.NotNull(producto);
        Assert.Equal(id, producto.Id);
        Assert.Equal(nombreEsperado, producto.Nombre);
        Assert.Equal(categoriaEsperada, producto.Categoria!.Nombre);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    [InlineData(-5)]
    public async Task ObtenerPorIdAsync_IdInexistente_RetornaNullYRegistraAdvertencia(int id)
    {
        var producto = await _servicio.ObtenerPorIdAsync(id);

        Assert.Null(producto);
        _loggerMock.VerificarLog(LogLevel.Warning, Times.Once());
    }

    // Registrar

    [Fact]
    public async Task AgregarAsync_ProductoValido_SeGuardaEnLaBaseDeDatos()
    {
        var nuevo = NuevoProducto();

        await _servicio.AgregarAsync(nuevo);

        Assert.True(nuevo.Id > 0);
        Assert.Equal(7, await _context.Productos.CountAsync());
        var guardado = await _context.Productos.FindAsync(nuevo.Id);
        Assert.NotNull(guardado);
        Assert.Equal("Tablet Z10", guardado.Nombre);
        Assert.Equal(299.99m, guardado.Precio);
        Assert.Equal(12, guardado.Stock);
        _loggerMock.VerificarLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task AgregarAsync_ProductoNulo_LanzaArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servicio.AgregarAsync(null!));
        Assert.Equal(6, await _context.Productos.CountAsync());
    }

    [Fact]
    public async Task AgregarAsync_ErrorDeBaseDeDatos_RegistraErrorYPropagaExcepcion()
    {
        _context.Dispose(); // simula una falla de la base de datos

        await Assert.ThrowsAsync<ObjectDisposedException>(() => _servicio.AgregarAsync(NuevoProducto()));
        _loggerMock.VerificarLogConExcepcion<ProductoService, ObjectDisposedException>(LogLevel.Error);
    }

    // Editar

    [Fact]
    public async Task EditarAsync_ProductoExistente_ActualizaTodosLosCampos()
    {
        var cambios = new Producto
        {
            Id = 1,
            Nombre = "Laptop Pro X (2026)",
            Descripcion = "Core i9, 32GB RAM, 1TB SSD",
            Precio = 1500m,
            Stock = 3,
            Estado = false,
            CategoriaId = 1,
            Imagen = "novabook-pro-14.jpg"
        };

        var resultado = await _servicio.EditarAsync(cambios);

        Assert.True(resultado);
        var editado = await _context.Productos.AsNoTracking().FirstAsync(p => p.Id == 1);
        Assert.Equal("Laptop Pro X (2026)", editado.Nombre);
        Assert.Equal("Core i9, 32GB RAM, 1TB SSD", editado.Descripcion);
        Assert.Equal(1500m, editado.Precio);
        Assert.Equal(3, editado.Stock);
        Assert.False(editado.Estado);
        _loggerMock.VerificarLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task EditarAsync_ProductoInexistente_RetornaFalseSinModificarDatos()
    {
        var cambios = NuevoProducto();
        cambios.Id = 500;

        var resultado = await _servicio.EditarAsync(cambios);

        Assert.False(resultado);
        Assert.Equal(6, await _context.Productos.CountAsync());
        Assert.DoesNotContain(await _context.Productos.ToListAsync(), p => p.Nombre == cambios.Nombre);
        _loggerMock.VerificarLog(LogLevel.Warning, Times.Once());
    }

    // Eliminar

    [Fact]
    public async Task EliminarAsync_ProductoExistente_LoEliminaYRetornaTrue()
    {
        var resultado = await _servicio.EliminarAsync(3);

        Assert.True(resultado);
        Assert.Equal(5, await _context.Productos.CountAsync());
        Assert.Null(await _context.Productos.FindAsync(3));
        _loggerMock.VerificarLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task EliminarAsync_ProductoInexistente_RetornaFalse()
    {
        var resultado = await _servicio.EliminarAsync(12345);

        Assert.False(resultado);
        Assert.Equal(6, await _context.Productos.CountAsync());
        _loggerMock.VerificarLog(LogLevel.Warning, Times.Once());
    }
}
