using Microsoft.EntityFrameworkCore;
using TechStore1.Data;

namespace TechStore1.Tests.Infraestructura;

// DbContext en memoria para pruebas
public static class TestDbContextFactory
{
    public static ApplicationDbContext Crear(string? nombre = null)
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(nombre ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(opciones);
        context.Database.EnsureCreated();
        return context;
    }
}
