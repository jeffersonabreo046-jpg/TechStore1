using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Playwright;
using TechStore1.Data;

namespace TechStore1.E2ETests.Infraestructura;

// Servidor Kestrel y navegador para las pruebas E2E
public class TechStoreServidorFixture : IAsyncLifetime
{
    private TechStoreKestrelFactory? _factory;
    private IPlaywright? _playwright;

    public IBrowser Navegador { get; private set; } = null!;
    public string UrlBase { get; private set; } = string.Empty;
    public string CarpetaEvidencias { get; } = ObtenerCarpetaEvidencias();

    public async Task InitializeAsync()
    {
        var puerto = PuertoLibre();
        UrlBase = $"http://localhost:{puerto}";
        _factory = new TechStoreKestrelFactory();
        _factory.UseKestrel(puerto);
        _factory.StartServer();

        _playwright = await Playwright.CreateAsync();
        Navegador = await LanzarNavegadorAsync(_playwright);
        Directory.CreateDirectory(CarpetaEvidencias);
    }

    public async Task DisposeAsync()
    {
        if (Navegador != null) await Navegador.DisposeAsync();
        _playwright?.Dispose();
        if (_factory != null) await _factory.DisposeAsync();
    }

    public async Task<IPage> NuevaPaginaAsync()
    {
        var contexto = await Navegador.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = UrlBase,
            ViewportSize = new ViewportSize { Width = 1366, Height = 900 },
            Locale = "es-SV"
        });
        return await contexto.NewPageAsync();
    }

    public Task CapturaAsync(IPage pagina, string nombre) =>
        pagina.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(CarpetaEvidencias, $"{nombre}.png"),
            FullPage = true
        });

    private static async Task<IBrowser> LanzarNavegadorAsync(IPlaywright playwright)
    {
        var headless = Environment.GetEnvironmentVariable("HEADED") != "1";
        var slowMo = headless ? 0 : 400;

        // Navegadores instalados
        foreach (var canal in new[] { "msedge", "chrome" })
        {
            try
            {
                return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Channel = canal, Headless = headless, SlowMo = slowMo
                });
            }
            catch (PlaywrightException) { }
        }

        // Chromium de Playwright
        Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = headless, SlowMo = slowMo });
    }

    private static int PuertoLibre()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var puerto = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return puerto;
    }

    private static string ObtenerCarpetaEvidencias()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "TechStore1.slnx")))
        {
            dir = dir.Parent;
        }
        return Path.Combine(dir?.FullName ?? AppContext.BaseDirectory, "Evidencias", "E2E");
    }

    private class TechStoreKestrelFactory : WebApplicationFactory<Program>
    {
        private readonly string _nombreBd = $"TechStoreE2E-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseStaticWebAssets(); // sirve wwwroot (bootstrap, site.css) fuera de Development
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(_nombreBd));
            });
        }
    }
}

[CollectionDefinition(Nombre)]
public class E2ECollection : ICollectionFixture<TechStoreServidorFixture>
{
    public const string Nombre = "E2E TechStore";
}
