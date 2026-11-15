using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using TechStore1.Authorization;
using TechStore1.Controllers;

namespace TechStore1.Tests.Unitarias;

// Pruebas de los atributos de autorización
[Trait("Categoria", "Unitaria")]
public class AutorizacionAtributosTests
{
    private static AuthorizeAttribute? AtributoClase<T>() =>
        typeof(T).GetCustomAttribute<AuthorizeAttribute>();

    private static IEnumerable<AuthorizeAttribute> AtributosAccion<T>(string accion) =>
        typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == accion)
            .SelectMany(m => m.GetCustomAttributes<AuthorizeAttribute>());

    [Fact]
    public void ProductosController_SoloAdministradorYVendedor()
    {
        var atributo = AtributoClase<ProductosController>();

        Assert.NotNull(atributo);
        Assert.Equal(AppRoles.AdministradorOVendedor, atributo.Roles);
        Assert.DoesNotContain(AppRoles.Cliente, atributo.Roles!);
    }

    [Theory]
    [InlineData(nameof(ProductosController.Delete))]
    [InlineData(nameof(ProductosController.DeleteConfirmed))]
    public void ProductosController_EliminarSoloAdministrador(string accion)
    {
        var atributos = AtributosAccion<ProductosController>(accion).ToList();

        Assert.NotEmpty(atributos);
        Assert.All(atributos, a => Assert.Equal(AppRoles.Administrador, a.Roles));
    }

    [Fact]
    public void ReportesController_SoloAdministradorYVendedor()
    {
        Assert.Equal(AppRoles.AdministradorOVendedor, AtributoClase<ReportesController>()?.Roles);
    }

    [Theory]
    [InlineData(typeof(UsuariosController))]
    [InlineData(typeof(RegistrosController))]
    public void ControladoresDeAdministracion_SoloAdministrador(Type controlador)
    {
        var atributo = controlador.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(atributo);
        Assert.Equal(AppRoles.Administrador, atributo.Roles);
    }

    [Theory]
    [InlineData(nameof(HomeController.Productos))]
    [InlineData(nameof(HomeController.Detalle))]
    [InlineData(nameof(HomeController.Categorias))]
    public void ConsultaDeCatalogo_RequiereUsuarioAutenticado(string accion)
    {
        var atributos = AtributosAccion<HomeController>(accion).ToList();

        Assert.Single(atributos);
        Assert.Null(atributos[0].Roles); // cualquier rol autenticado
    }

    [Fact]
    public void AppRoles_DefineLosTresRolesRequeridos()
    {
        Assert.Equal(new[] { "Administrador", "Vendedor", "Cliente" }, AppRoles.Todos);
    }
}
