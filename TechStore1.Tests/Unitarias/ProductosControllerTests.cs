using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using TechStore1.Controllers;
using TechStore1.Models;
using TechStore1.Services;
using TechStore1.Tests.Infraestructura;

namespace TechStore1.Tests.Unitarias;

// Pruebas unitarias de ProductosController
[Trait("Categoria", "Unitaria")]
public class ProductosControllerTests
{
    private readonly Mock<IProductoService> _productoServiceMock = new();
    private readonly Mock<ICategoriaService> _categoriaServiceMock = new();
    private readonly Mock<ILogger<ProductosController>> _loggerMock = new();
    private readonly ProductosController _controller;

    private static readonly List<Categoria> Categorias = new()
    {
        new Categoria { Id = 1, Nombre = "Laptops" },
        new Categoria { Id = 2, Nombre = "Smartphones" }
    };

    public ProductosControllerTests()
    {
        _categoriaServiceMock.Setup(s => s.ObtenerTodasAsync()).ReturnsAsync(Categorias);

        _controller = new ProductosController(_productoServiceMock.Object, _categoriaServiceMock.Object, _loggerMock.Object);

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "vendedor"),
                new Claim(ClaimTypes.Role, "Vendedor")
            }, "Test"))
        };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    private static Producto ProductoValido(int id = 0) => new()
    {
        Id = id,
        Nombre = "Laptop Pro X",
        Descripcion = "Core i7",
        Precio = 1200m,
        Stock = 10,
        Estado = true,
        CategoriaId = 1
    };

    [Fact]
    public async Task Index_RetornaVistaConLosProductosDelServicio()
    {
        var productos = new List<Producto> { ProductoValido(1), ProductoValido(2) };
        _productoServiceMock.Setup(s => s.ObtenerTodosAsync()).ReturnsAsync(productos);

        var resultado = await _controller.Index();

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsAssignableFrom<IEnumerable<Producto>>(vista.Model);
        Assert.Equal(2, modelo.Count());
        _productoServiceMock.Verify(s => s.ObtenerTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateGet_RetornaVistaConProductoActivoYCategorias()
    {
        var resultado = await _controller.Create();

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<Producto>(vista.Model);
        Assert.True(modelo.Estado);
        var lista = Assert.IsType<SelectList>((object)_controller.ViewBag.Categorias);
        Assert.Equal(2, lista.Count());
    }

    [Fact]
    public async Task CreatePost_ModeloValido_LlamaAgregarYRedirigeAIndex()
    {
        var producto = ProductoValido();

        var resultado = await _controller.Create(producto);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(ProductosController.Index), redireccion.ActionName);
        Assert.Equal("Producto agregado correctamente.", _controller.TempData["Mensaje"]);
        _productoServiceMock.Verify(s => s.AgregarAsync(producto), Times.Once);
    }

    [Fact]
    public async Task CreatePost_ModeloInvalido_RetornaVistaYNoGuarda()
    {
        var producto = ProductoValido();
        _controller.ModelState.AddModelError("Nombre", "El nombre es obligatorio");

        var resultado = await _controller.Create(producto);

        var vista = Assert.IsType<ViewResult>(resultado);
        Assert.Same(producto, vista.Model);
        Assert.False(_controller.ModelState.IsValid);
        _productoServiceMock.Verify(s => s.AgregarAsync(It.IsAny<Producto>()), Times.Never);
    }

    [Fact]
    public async Task CreatePost_ErrorEnServicio_RetornaVistaConMensajeYRegistraError()
    {
        _productoServiceMock.Setup(s => s.AgregarAsync(It.IsAny<Producto>()))
            .ThrowsAsync(new InvalidOperationException("Falla de BD"));

        var resultado = await _controller.Create(ProductoValido());

        Assert.IsType<ViewResult>(resultado);
        Assert.False(_controller.ModelState.IsValid);
        Assert.Contains(_controller.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage.Contains("error al guardar"));
        _loggerMock.VerificarLogConExcepcion<ProductosController, InvalidOperationException>(LogLevel.Error);
    }

    [Fact]
    public async Task EditGet_ProductoExistente_RetornaVistaConProducto()
    {
        _productoServiceMock.Setup(s => s.ObtenerPorIdAsync(1)).ReturnsAsync(ProductoValido(1));

        var resultado = await _controller.Edit(1);

        var vista = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<Producto>(vista.Model);
        Assert.Equal(1, modelo.Id);
    }

    [Fact]
    public async Task EditGet_ProductoInexistente_RetornaNotFound()
    {
        _productoServiceMock.Setup(s => s.ObtenerPorIdAsync(99)).ReturnsAsync((Producto?)null);

        var resultado = await _controller.Edit(99);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task EditPost_IdNoCoincide_RetornaNotFoundSinEditar()
    {
        var resultado = await _controller.Edit(5, ProductoValido(1));

        Assert.IsType<NotFoundResult>(resultado);
        _productoServiceMock.Verify(s => s.EditarAsync(It.IsAny<Producto>()), Times.Never);
    }

    [Fact]
    public async Task EditPost_ModeloValido_EditaYRedirige()
    {
        var producto = ProductoValido(1);
        _productoServiceMock.Setup(s => s.EditarAsync(producto)).ReturnsAsync(true);

        var resultado = await _controller.Edit(1, producto);

        var redireccion = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redireccion.ActionName);
        Assert.Equal("Producto actualizado correctamente.", _controller.TempData["Mensaje"]);
        _productoServiceMock.Verify(s => s.EditarAsync(producto), Times.Once);
    }

    [Fact]
    public async Task EditPost_ProductoNoExisteEnServicio_RetornaNotFound()
    {
        _productoServiceMock.Setup(s => s.EditarAsync(It.IsAny<Producto>())).ReturnsAsync(false);

        var resultado = await _controller.Edit(1, ProductoValido(1));

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task DeleteGet_ProductoExistente_RetornaVistaDeConfirmacion()
    {
        _productoServiceMock.Setup(s => s.ObtenerPorIdAsync(3)).ReturnsAsync(ProductoValido(3));

        var resultado = await _controller.Delete(3);

        var vista = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(3, Assert.IsType<Producto>(vista.Model).Id);
    }

    [Fact]
    public async Task DeleteConfirmed_ProductoExistente_EliminaYRedirige()
    {
        _productoServiceMock.Setup(s => s.EliminarAsync(3)).ReturnsAsync(true);

        var resultado = await _controller.DeleteConfirmed(3);

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Producto eliminado correctamente.", _controller.TempData["Mensaje"]);
        _productoServiceMock.Verify(s => s.EliminarAsync(3), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_ProductoInexistente_RetornaNotFound()
    {
        _productoServiceMock.Setup(s => s.EliminarAsync(It.IsAny<int>())).ReturnsAsync(false);

        var resultado = await _controller.DeleteConfirmed(77);

        Assert.IsType<NotFoundResult>(resultado);
    }
}
