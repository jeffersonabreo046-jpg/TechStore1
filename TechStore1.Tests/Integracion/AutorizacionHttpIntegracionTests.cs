using System.Net;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Testing;
using TechStore1.Tests.Infraestructura;

namespace TechStore1.Tests.Integracion;

// Pruebas de integración HTTP
[Trait("Categoria", "Integracion")]
public class AutorizacionHttpIntegracionTests : IClassFixture<TechStoreWebApplicationFactory>
{
    private readonly TechStoreWebApplicationFactory _factory;

    public AutorizacionHttpIntegracionTests(TechStoreWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CrearCliente() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private async Task<HttpClient> ClienteAutenticadoAsync(string usuario, string contrasena)
    {
        var cliente = CrearCliente();
        var respuesta = await cliente.IniciarSesionAsync(usuario, contrasena);
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        return cliente;
    }

    [Theory]
    [InlineData("/Productos")]
    [InlineData("/Productos/Create")]
    [InlineData("/Reportes/Pdf")]
    [InlineData("/Usuarios")]
    [InlineData("/Home/Productos")]
    public async Task UsuarioAnonimo_RutaProtegida_RedirigeALogin(string ruta)
    {
        var respuesta = await CrearCliente().GetAsync(ruta);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("/Account/Login", respuesta.Headers.Location!.PathAndQuery, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_CredencialesIncorrectas_MuestraMensajeDeError()
    {
        var cliente = CrearCliente();

        var respuesta = await cliente.IniciarSesionAsync("admin", "incorrecta");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Usuario o contraseña incorrectos", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Cliente_PuedeConsultarCatalogoPeroNoAdministrar()
    {
        var cliente = await ClienteAutenticadoAsync("cliente1", "cliente1");

        var catalogo = await cliente.GetAsync("/Home/Productos");
        Assert.Equal(HttpStatusCode.OK, catalogo.StatusCode);
        var html = await catalogo.Content.ReadAsStringAsync();
        Assert.Contains("Laptop Pro X", html);
        Assert.DoesNotContain("Administrar productos", html); // menú según rol

        foreach (var ruta in new[] { "/Productos", "/Productos/Create", "/Productos/Edit/1", "/Reportes/Excel", "/Usuarios" })
        {
            var respuesta = await cliente.GetAsync(ruta);
            Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
            Assert.StartsWith("/Account/AccessDenied", respuesta.Headers.Location!.PathAndQuery, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task PaginaAccesoDenegado_MuestraMensaje()
    {
        var cliente = await ClienteAutenticadoAsync("cliente1", "cliente1");

        var respuesta = await cliente.GetAsync("/Account/AccessDenied?ReturnUrl=%2FProductos");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Contains("Acceso denegado", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Vendedor_PuedeGestionarProductosPeroNoEliminar()
    {
        var cliente = await ClienteAutenticadoAsync("vendedor", "vendedor123");

        var index = await cliente.GetAsync("/Productos");
        Assert.Equal(HttpStatusCode.OK, index.StatusCode);
        var html = await index.Content.ReadAsStringAsync();
        Assert.Contains("Administrar productos", html);
        Assert.DoesNotContain(">Eliminar<", html.Replace(" ", "").Replace("\n", "").Replace("\r", ""));

        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/Productos/Create")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/Productos/Edit/1")).StatusCode);

        var eliminar = await cliente.GetAsync("/Productos/Delete/1");
        Assert.Equal(HttpStatusCode.Redirect, eliminar.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", eliminar.Headers.Location!.PathAndQuery);

        var usuarios = await cliente.GetAsync("/Usuarios");
        Assert.StartsWith("/Account/AccessDenied", usuarios.Headers.Location!.PathAndQuery);
    }

    [Fact]
    public async Task Vendedor_RegistraProductoMedianteFormulario()
    {
        var cliente = await ClienteAutenticadoAsync("vendedor", "vendedor123");
        var token = await cliente.ObtenerTokenAntiforgeryAsync("/Productos/Create");

        var respuesta = await cliente.PostAsync("/Productos/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Nombre"] = "Parlante Bluetooth HTTP",
            ["Descripcion"] = "Creado desde prueba de integración",
            ["Precio"] = "59.90",
            ["Stock"] = "14",
            ["CategoriaId"] = "3",
            ["Estado"] = "true",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        var listado = await cliente.GetStringAsync("/Productos");
        Assert.Contains("Parlante Bluetooth HTTP", listado);
    }

    [Fact]
    public async Task Administrador_TieneAccesoCompleto()
    {
        var cliente = await ClienteAutenticadoAsync("admin", "admin123");

        foreach (var ruta in new[] { "/Productos", "/Productos/Delete/2", "/Usuarios", "/Usuarios/Create", "/Registros" })
        {
            var respuesta = await cliente.GetAsync(ruta);
            Assert.True(respuesta.StatusCode == HttpStatusCode.OK, $"{ruta} devolvió {respuesta.StatusCode}");
        }
    }

    [Fact]
    public async Task ReportePdf_GeneraDocumentoPdfDescargable()
    {
        var cliente = await ClienteAutenticadoAsync("vendedor", "vendedor123");

        var respuesta = await cliente.GetAsync("/Reportes/Pdf");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("application/pdf", respuesta.Content.Headers.ContentType!.MediaType);
        Assert.EndsWith(".pdf", respuesta.Content.Headers.ContentDisposition!.FileNameStar ?? respuesta.Content.Headers.ContentDisposition.FileName);
        var bytes = await respuesta.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 1000);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task ReporteExcel_ContieneEncabezadosYProductosDeLaBaseDeDatos()
    {
        var cliente = await ClienteAutenticadoAsync("admin", "admin123");

        var respuesta = await cliente.GetAsync("/Reportes/Excel");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", respuesta.Content.Headers.ContentType!.MediaType);

        using var stream = await respuesta.Content.ReadAsStreamAsync();
        using var libro = new XLWorkbook(stream);
        var hoja = libro.Worksheet("Productos");
        var encabezados = Enumerable.Range(1, 7).Select(c => hoja.Cell(4, c).GetString()).ToArray();
        Assert.Equal(new[] { "ID", "Nombre", "Descripción", "Precio", "Categoría", "Stock", "Estado" }, encabezados);

        var nombres = hoja.Column(2).CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("Laptop Pro X", nombres);
        Assert.Contains("Teclado Mecánico", nombres);
    }

    [Fact]
    public async Task RutaInexistente_MuestraPaginaDeErrorPersonalizada404()
    {
        var respuesta = await CrearCliente().GetAsync("/ruta/que/no/existe");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Contains("Página no encontrada", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ExcepcionNoControlada_MuestraPaginaDeErrorPersonalizada500()
    {
        var cliente = await ClienteAutenticadoAsync("admin", "admin123");

        var respuesta = await cliente.GetAsync("/Registros/ProbarError");

        Assert.Equal(HttpStatusCode.InternalServerError, respuesta.StatusCode);
        var html = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("Ocurrió un error inesperado", html);
        Assert.DoesNotContain("InvalidOperationException", html); // no se exponen detalles internos
    }
}
