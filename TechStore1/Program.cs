using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.WebEncoders;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TechStore1.Authorization;
using TechStore1.Data;
using TechStore1.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging con Serilog (consola y carpeta Logs)
var carpetaLogs = Path.Combine(builder.Environment.ContentRootPath, "Logs");
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore.HttpsPolicy", LogEventLevel.Error)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

    if (!context.HostingEnvironment.IsEnvironment("Testing"))
    {
        configuration.WriteTo.File(
            Path.Combine(carpetaLogs, "techstore-.log"),
            rollingInterval: RollingInterval.Day,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
    }
});

// Add services to the container.
builder.Services.AddControllersWithViews();
// Mostrar tildes y ñ sin codificar
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 5;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppRoles.GestionProductos, policy =>
        policy.RequireRole(AppRoles.Administrador, AppRoles.Vendedor));
    options.AddPolicy(AppRoles.EliminacionProductos, policy =>
        policy.RequireRole(AppRoles.Administrador));
    options.AddPolicy(AppRoles.ConsultaReportes, policy =>
        policy.RequireRole(AppRoles.Administrador, AppRoles.Vendedor));
    options.AddPolicy(AppRoles.AdministracionGeneral, policy =>
        policy.RequireRole(AppRoles.Administrador));
});

builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddSingleton(new LogFileOptions(carpetaLogs));

var app = builder.Build();

// Migraciones, roles y usuarios
await DbSeeder.InicializarAsync(app.Services);

// Páginas de error
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging(options =>
{
    // No registrar archivos estáticos
    options.GetLevel = (context, _, ex) =>
        ex != null || context.Response.StatusCode >= 500 ? LogEventLevel.Error
        : Path.HasExtension(context.Request.Path.Value) ? LogEventLevel.Verbose
        : LogEventLevel.Information;
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

// Necesario para las pruebas de integración
public partial class Program { }
