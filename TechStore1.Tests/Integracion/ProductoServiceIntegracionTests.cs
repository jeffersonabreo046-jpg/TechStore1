using Microsoft.Extensions.DependencyInjection;
using TechStore1.Models;
using TechStore1.Services;
using TechStore1.Tests.Infraestructura;

namespace TechStore1.Tests.Integracion;

// Prueba de integración del servicio de productos
[Trait("Categoria", "Integracion")]
public class ProductoServiceIntegracionTests : IClassFixture<TechStoreWebApplicationFactory>
{
    private readonly TechStoreWebApplicationFactory _factory;

    public ProductoServiceIntegracionTests(TechStoreWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CicloCompletoCrud_ServicioRegistradoEnDI_FuncionaConLaBaseDeDatos()
    {
        // Registrar
        int idNuevo;
        using (var scope = _factory.Services.CreateScope())
        {
            var servicio = scope.ServiceProvider.GetRequiredService<IProductoService>();
            Assert.IsType<ProductoService>(servicio);

            var nuevo = new Producto
            {
                Nombre = "Integración - Webcam HD",
                Descripcion = "Webcam 1080p",
                Precio = 45.50m,
                Stock = 8,
                Estado = true,
                CategoriaId = 3
            };
            await servicio.AgregarAsync(nuevo);
            idNuevo = nuevo.Id;
            Assert.True(idNuevo > 0);
        }

        // Consultar con un nuevo DbContext
        using (var scope = _factory.Services.CreateScope())
        {
            var servicio = scope.ServiceProvider.GetRequiredService<IProductoService>();
            var todos = await servicio.ObtenerTodosAsync();
            var encontrado = await servicio.ObtenerPorIdAsync(idNuevo);

            Assert.Contains(todos, p => p.Id == idNuevo);
            Assert.NotNull(encontrado);
            Assert.Equal("Accesorios", encontrado.Categoria?.Nombre);

            // Editar
            encontrado.Precio = 39.99m;
            encontrado.Stock = 20;
            Assert.True(await servicio.EditarAsync(encontrado));
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var servicio = scope.ServiceProvider.GetRequiredService<IProductoService>();
            var editado = await servicio.ObtenerPorIdAsync(idNuevo);
            Assert.Equal(39.99m, editado!.Precio);
            Assert.Equal(20, editado.Stock);

            // Eliminar
            Assert.True(await servicio.EliminarAsync(idNuevo));
            Assert.Null(await servicio.ObtenerPorIdAsync(idNuevo));
        }
    }

    [Fact]
    public async Task Seeder_CreaRolesYUsuariosConIdentity()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();

        foreach (var rol in Authorization.AppRoles.Todos)
        {
            Assert.True(await roleManager.RoleExistsAsync(rol), $"Falta el rol {rol}");
        }

        var admin = await userManager.FindByNameAsync("admin");
        var vendedor = await userManager.FindByNameAsync("vendedor");
        var cliente = await userManager.FindByNameAsync("cliente1");

        Assert.True(await userManager.IsInRoleAsync(admin!, "Administrador"));
        Assert.True(await userManager.IsInRoleAsync(vendedor!, "Vendedor"));
        Assert.True(await userManager.IsInRoleAsync(cliente!, "Cliente"));
        Assert.False(await userManager.IsInRoleAsync(cliente!, "Administrador"));
    }
}
