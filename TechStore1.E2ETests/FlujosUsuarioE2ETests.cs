using System.Text.RegularExpressions;
using Microsoft.Playwright;
using TechStore1.E2ETests.Infraestructura;
using static Microsoft.Playwright.Assertions;

namespace TechStore1.E2ETests;

// Pruebas E2E con Playwright
[Collection(E2ECollection.Nombre)]
[Trait("Categoria", "E2E")]
public class FlujosUsuarioE2ETests
{
    private readonly TechStoreServidorFixture _fx;

    public FlujosUsuarioE2ETests(TechStoreServidorFixture fx)
    {
        _fx = fx;
    }

    private static async Task IniciarSesionAsync(IPage pagina, string usuario, string contrasena)
    {
        await pagina.GotoAsync("/Account/Login");
        await pagina.GetByLabel("Usuario").FillAsync(usuario);
        await pagina.GetByLabel("Contraseña").FillAsync(contrasena);
        // Esperar la respuesta del login
        await pagina.RunAndWaitForResponseAsync(
            () => pagina.GetByRole(AriaRole.Button, new() { Name = "Ingresar" }).ClickAsync(),
            r => r.Url.Contains("/Account/Login") && r.Request.Method == "POST");
        await pagina.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    // Inicio de sesión

    [Fact]
    public async Task InicioSesion_CredencialesValidas_MuestraUsuarioYRol()
    {
        var pagina = await _fx.NuevaPaginaAsync();

        await pagina.GotoAsync("/Account/Login");
        await _fx.CapturaAsync(pagina, "01_login_formulario");
        await IniciarSesionAsync(pagina, "admin", "admin123");

        await Expect(pagina).ToHaveURLAsync(new Regex("/$|/Home"));
        await Expect(pagina.Locator("#usuario-actual")).ToContainTextAsync("admin");
        await Expect(pagina.Locator("#usuario-rol")).ToHaveTextAsync("Administrador");
        await _fx.CapturaAsync(pagina, "02_login_exitoso_admin");
    }

    [Fact]
    public async Task InicioSesion_CredencialesInvalidas_MuestraMensajeDeError()
    {
        var pagina = await _fx.NuevaPaginaAsync();

        await IniciarSesionAsync(pagina, "admin", "clave-incorrecta");

        await Expect(pagina).ToHaveURLAsync(new Regex("/Account/Login"));
        await Expect(pagina.GetByText("Usuario o contraseña incorrectos.")).ToBeVisibleAsync();
        await _fx.CapturaAsync(pagina, "03_login_error_credenciales");
    }

    [Fact]
    public async Task UsuarioAnonimo_RutaProtegida_RedirigeAlLogin()
    {
        var pagina = await _fx.NuevaPaginaAsync();

        await pagina.GotoAsync("/Productos");

        await Expect(pagina).ToHaveURLAsync(new Regex("/Account/Login\\?ReturnUrl=%2FProductos", RegexOptions.IgnoreCase));
        await Expect(pagina.GetByRole(AriaRole.Heading, new() { Name = "Iniciar sesión" })).ToBeVisibleAsync();
    }

    // Acceso según rol y consulta de productos

    [Fact]
    public async Task Cliente_ConsultaProductos_YMenuSinOpcionesDeAdministracion()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "cliente1", "cliente1");

        await Expect(pagina.Locator("#usuario-rol")).ToHaveTextAsync("Cliente");
        await Expect(pagina.Locator("#menu-admin-productos")).ToHaveCountAsync(0);
        await Expect(pagina.Locator("#menu-reportes")).ToHaveCountAsync(0);
        await Expect(pagina.Locator("#menu-administracion")).ToHaveCountAsync(0);
        await _fx.CapturaAsync(pagina, "04_menu_cliente");

        await pagina.GotoAsync("/Home/Productos");
        await Expect(pagina.GetByRole(AriaRole.Heading, new() { Name = "Catálogo de Productos" })).ToBeVisibleAsync();
        await Expect(pagina.GetByText("Laptop Pro X")).ToBeVisibleAsync();
        await _fx.CapturaAsync(pagina, "05_catalogo_cliente");

        // Detalle de un producto
        await pagina.Locator(".card", new() { HasText = "Monitor 4K 27" })
            .GetByRole(AriaRole.Link, new() { Name = "Ver detalle" }).ClickAsync();
        await Expect(pagina.Locator("#detalle-nombre")).ToHaveTextAsync("Monitor 4K 27");
        await Expect(pagina.GetByRole(AriaRole.Link, new() { Name = "Editar" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Vendedor_MenuConProductosYReportes_SinAdministracion()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "vendedor", "vendedor123");

        await Expect(pagina.Locator("#usuario-rol")).ToHaveTextAsync("Vendedor");
        await Expect(pagina.Locator("#menu-admin-productos")).ToBeVisibleAsync();
        await Expect(pagina.Locator("#menu-reportes")).ToBeVisibleAsync();
        await Expect(pagina.Locator("#menu-administracion")).ToHaveCountAsync(0);
        await _fx.CapturaAsync(pagina, "06_menu_vendedor");
    }

    [Fact]
    public async Task Administrador_MenuCompleto_YAdministraUsuarios()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "admin", "admin123");

        await Expect(pagina.Locator("#menu-admin-productos")).ToBeVisibleAsync();
        await Expect(pagina.Locator("#menu-reportes")).ToBeVisibleAsync();
        await pagina.Locator("#menu-administracion").ClickAsync();
        await Expect(pagina.Locator("#menu-usuarios")).ToBeVisibleAsync();
        await _fx.CapturaAsync(pagina, "07_menu_administrador");

        await pagina.Locator("#menu-usuarios").ClickAsync();
        await Expect(pagina.GetByRole(AriaRole.Heading, new() { Name = "Usuarios y roles" })).ToBeVisibleAsync();
        await Expect(pagina.Locator("#tabla-usuarios")).ToContainTextAsync("vendedor");
        await Expect(pagina.Locator("#tabla-usuarios")).ToContainTextAsync("cliente1");
        await _fx.CapturaAsync(pagina, "08_usuarios_y_roles");
    }

    // Registro y edición

    [Fact]
    public async Task Vendedor_RegistraYEditaProducto()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "vendedor", "vendedor123");
        var nombre = $"Smartwatch E2E {Guid.NewGuid().ToString()[..5]}";

        // Registro
        await pagina.GotoAsync("/Productos");
        await Expect(pagina.Locator("#resumen-inventario")).ToBeVisibleAsync(); // View Component
        await pagina.GetByRole(AriaRole.Link, new() { Name = "Agregar producto" }).ClickAsync();
        await pagina.GetByLabel("Nombre", new() { Exact = true }).FillAsync(nombre);
        await pagina.GetByLabel("Descripción").FillAsync("Reloj inteligente con GPS");
        await pagina.GetByLabel("Precio").FillAsync("149.99");
        await pagina.GetByLabel("Stock").FillAsync("7");
        await pagina.GetByLabel("Categoría").SelectOptionAsync(new SelectOptionValue { Label = "Accesorios" });
        await _fx.CapturaAsync(pagina, "09_formulario_registro_producto");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar producto" }).ClickAsync();

        await Expect(pagina.Locator("#alerta-exito")).ToContainTextAsync("Producto agregado correctamente.");
        var fila = pagina.Locator("#tabla-productos tr", new() { HasText = nombre });
        await Expect(fila).ToContainTextAsync("$149.99");
        await _fx.CapturaAsync(pagina, "10_producto_registrado");

        // Edición
        await fila.GetByRole(AriaRole.Link, new() { Name = "Editar" }).ClickAsync();
        await pagina.GetByLabel("Precio").FillAsync("129.50");
        await pagina.GetByLabel("Stock").FillAsync("25");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar cambios" }).ClickAsync();

        await Expect(pagina.Locator("#alerta-exito")).ToContainTextAsync("Producto actualizado correctamente.");
        await Expect(fila).ToContainTextAsync("$129.50");
        await Expect(fila).ToContainTextAsync("25");
        await _fx.CapturaAsync(pagina, "11_producto_editado");

        // El vendedor no ve el botón Eliminar
        await Expect(pagina.Locator("#tabla-productos").GetByRole(AriaRole.Link, new() { Name = "Eliminar" })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Vendedor_FormularioInvalido_MuestraValidaciones()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "vendedor", "vendedor123");

        await pagina.GotoAsync("/Productos/Create");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Guardar producto" }).ClickAsync();

        await Expect(pagina.GetByText("El nombre es obligatorio")).ToBeVisibleAsync();
        await Expect(pagina).ToHaveURLAsync(new Regex("/Productos/Create"));
        await _fx.CapturaAsync(pagina, "12_validaciones_formulario");
    }

    [Fact]
    public async Task Administrador_EliminaProducto()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "admin", "admin123");

        await pagina.GotoAsync("/Productos");
        var fila = pagina.Locator("#tabla-productos tr", new() { HasText = "Mouse gamer RGB" });
        await fila.GetByRole(AriaRole.Link, new() { Name = "Eliminar" }).ClickAsync();
        await Expect(pagina.GetByText("¿Está seguro de eliminar este producto?")).ToBeVisibleAsync();
        await _fx.CapturaAsync(pagina, "13_confirmar_eliminacion");
        await pagina.GetByRole(AriaRole.Button, new() { Name = "Sí, eliminar" }).ClickAsync();

        await Expect(pagina.Locator("#alerta-exito")).ToContainTextAsync("Producto eliminado correctamente.");
        await Expect(pagina.Locator("#tabla-productos")).Not.ToContainTextAsync("Mouse gamer RGB");
    }

    // Restricción de acceso y errores

    [Theory]
    [InlineData("cliente1", "cliente1", "/Productos")]
    [InlineData("cliente1", "cliente1", "/Productos/Create")]
    [InlineData("cliente1", "cliente1", "/Reportes/Pdf")]
    [InlineData("vendedor", "vendedor123", "/Productos/Delete/1")]
    [InlineData("vendedor", "vendedor123", "/Usuarios")]
    [InlineData("vendedor", "vendedor123", "/Registros")]
    public async Task UsuarioSinPermisos_AccesoDirectoARuta_MuestraAccesoDenegado(string usuario, string contrasena, string ruta)
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, usuario, contrasena);

        await pagina.GotoAsync(ruta);

        await Expect(pagina).ToHaveURLAsync(new Regex("/Account/AccessDenied"));
        await Expect(pagina.GetByRole(AriaRole.Heading, new() { Name = "Acceso denegado" })).ToBeVisibleAsync();
        await Expect(pagina.GetByText("No tienes permisos para acceder")).ToBeVisibleAsync();
        await _fx.CapturaAsync(pagina, $"14_acceso_denegado_{usuario}_{ruta.Trim('/').Replace('/', '_')}");
    }

    [Fact]
    public async Task RutaInexistente_MuestraPaginaDeError404()
    {
        var pagina = await _fx.NuevaPaginaAsync();

        var respuesta = await pagina.GotoAsync("/pagina/que/no/existe");

        Assert.Equal(404, respuesta!.Status);
        await Expect(pagina.Locator("#error-titulo")).ToHaveTextAsync("Página no encontrada");
        await _fx.CapturaAsync(pagina, "15_error_404");
    }

    [Fact]
    public async Task ExcepcionNoControlada_MuestraPaginaDeErrorPersonalizada()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "admin", "admin123");

        var respuesta = await pagina.GotoAsync("/Registros/ProbarError");

        Assert.Equal(500, respuesta!.Status);
        await Expect(pagina.Locator("#error-titulo")).ToHaveTextAsync("Ocurrió un error inesperado");
        await _fx.CapturaAsync(pagina, "16_error_500_personalizado");
    }

    // Reportes

    [Fact]
    public async Task Vendedor_DescargaReportesPdfYExcel()
    {
        var pagina = await _fx.NuevaPaginaAsync();
        await IniciarSesionAsync(pagina, "vendedor", "vendedor123");
        await pagina.GotoAsync("/Productos");

        var descargaPdf = await pagina.RunAndWaitForDownloadAsync(() => pagina.Locator("#btn-pdf").ClickAsync());
        Assert.EndsWith(".pdf", descargaPdf.SuggestedFilename);

        var descargaExcel = await pagina.RunAndWaitForDownloadAsync(() => pagina.Locator("#btn-excel").ClickAsync());
        Assert.EndsWith(".xlsx", descargaExcel.SuggestedFilename);

        await descargaPdf.SaveAsAsync(Path.Combine(_fx.CarpetaEvidencias, "reporte_productos.pdf"));
        await descargaExcel.SaveAsAsync(Path.Combine(_fx.CarpetaEvidencias, "reporte_productos.xlsx"));
    }
}
