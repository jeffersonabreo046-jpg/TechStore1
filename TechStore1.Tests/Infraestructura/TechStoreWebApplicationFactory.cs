using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechStore1.Data;

namespace TechStore1.Tests.Infraestructura;

// Aplicación completa con base de datos en memoria
public class TechStoreWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _nombreBd = $"TechStoreTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Quitar la configuración de SQL Server
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_nombreBd));
        });
    }
}
