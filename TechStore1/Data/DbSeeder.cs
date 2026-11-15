using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore1.Authorization;

namespace TechStore1.Data
{
    // Crea la base de datos, los roles y los usuarios
    public static class DbSeeder
    {
        // Usuarios de prueba
        public static readonly (string Usuario, string Contrasena, string Rol)[] UsuariosDemo =
        {
            ("admin", "admin123", AppRoles.Administrador),
            ("vendedor", "vendedor123", AppRoles.Vendedor),
            ("cliente1", "cliente1", AppRoles.Cliente),
            ("cliente2", "cliente2", AppRoles.Cliente),
            ("cliente3", "cliente3", AppRoles.Cliente),
        };

        public static async Task InicializarAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("TechStore1.DbSeeder");

            try
            {
                var context = provider.GetRequiredService<ApplicationDbContext>();
                if (context.Database.IsRelational())
                {
                    // BD creada con una migración antigua
                    var aplicadas = (await context.Database.GetAppliedMigrationsAsync()).ToList();
                    var conocidas = context.Database.GetMigrations().ToList();
                    if (aplicadas.Count > 0 && !aplicadas.Intersect(conocidas).Any())
                    {
                        logger.LogError(
                            "La base de datos fue creada con migraciones antiguas ({Aplicadas}) incompatibles con las actuales ({Conocidas}). " +
                            "Elimínela con 'dotnet ef database drop' (o desde SQL Server Object Explorer) y vuelva a ejecutar la aplicación.",
                            string.Join(", ", aplicadas), string.Join(", ", conocidas));
                        return;
                    }

                    await context.Database.MigrateAsync();
                }
                else
                {
                    await context.Database.EnsureCreatedAsync();
                }

                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

                // Crear roles
                foreach (var rol in AppRoles.Todos)
                {
                    if (!await roleManager.RoleExistsAsync(rol))
                    {
                        await roleManager.CreateAsync(new IdentityRole(rol));
                        logger.LogInformation("Rol {Rol} creado", rol);
                    }
                }

                // Crear usuarios y asignar rol
                foreach (var (nombre, contrasena, rol) in UsuariosDemo)
                {
                    var usuario = await userManager.FindByNameAsync(nombre);
                    if (usuario == null)
                    {
                        usuario = new IdentityUser { UserName = nombre, Email = $"{nombre}@techstore.com" };
                        var resultado = await userManager.CreateAsync(usuario, contrasena);
                        if (!resultado.Succeeded)
                        {
                            logger.LogWarning("No se pudo crear el usuario {Usuario}: {Errores}", nombre,
                                string.Join(", ", resultado.Errors.Select(e => e.Description)));
                            continue;
                        }
                        logger.LogInformation("Usuario {Usuario} creado", nombre);
                    }

                    if (!await userManager.IsInRoleAsync(usuario, rol))
                    {
                        await userManager.AddToRoleAsync(usuario, rol);
                        logger.LogInformation("Rol {Rol} asignado al usuario {Usuario}", rol, nombre);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al inicializar la base de datos");
            }
        }
    }
}
