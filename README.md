# TechStore – Unidad III

ASP.NET Core MVC (.NET 10) + Identity + EF Core (SQL Server LocalDB).

Repositorio: https://github.com/jeffersonabreo046-jpg/TechStore1

## Ejecutar

```bash
cd TechStore1
dotnet run --launch-profile http   # http://localhost:5174
```

Al iniciar, `Data/DbSeeder.cs` aplica las migraciones y asegura roles/usuarios con `RoleManager` y `UserManager`.

> Si la BD local `TechStoreDB` fue creada con una migración antigua (Unidad II), el log mostrará un error.
> Elimínela (`dotnet ef database drop` o SQL Server Object Explorer) y vuelva a ejecutar.

## Usuarios de prueba

| Usuario  | Contraseña  | Rol           | Permisos |
|----------|-------------|---------------|----------|
| admin    | admin123    | Administrador | Todo: productos (incl. eliminar), reportes, usuarios y roles, registros |
| vendedor | vendedor123 | Vendedor      | Consultar, agregar y editar productos; reportes PDF/Excel |
| cliente1 | cliente1    | Cliente       | Solo consultar productos (catálogo y detalle) |

## Dónde está cada requisito

| Requisito | Ubicación |
|-----------|-----------|
| Autorización / `[Authorize]` | `Controllers/*` (`[Authorize]`, `[Authorize(Roles = ...)]`), `Program.cs` (cookie, LoginPath, AccessDeniedPath) |
| Roles y permisos | `Authorization/AppRoles.cs`, `Data/DbSeeder.cs`, `Controllers/UsuariosController.cs` (crear usuario / cambiar rol) |
| Menú según rol | `Views/Shared/_Layout.cshtml` |
| Logging (ILogger + Serilog) | Servicios y controladores; archivo `TechStore1/Logs/techstore-AAAAMMDD.log`; visor en **Administración → Registros** |
| Manejo de excepciones | try/catch en servicios y controladores; `UseExceptionHandler` + `UseStatusCodePagesWithReExecute` |
| Página de error personalizada | `Views/Shared/Error.cshtml` (404/500). Demo: **Administración → Registros → Generar error de prueba** |
| Acceso denegado | `Views/Account/AccessDenied.cshtml` |
| Partial Views | `_ProductCard`, `_EstadoBadge`, `_Alertas`, `Productos/_ProductoForm` |
| View Components | `ViewComponents/ResumenInventarioViewComponent` (Administrar productos), `UsuarioActualViewComponent` (navbar) |
| Reporte PDF (QuestPDF) | `Controllers/ReportesController.cs` → `/Reportes/Pdf` |
| Excel (ClosedXML) | `Controllers/ReportesController.cs` → `/Reportes/Excel` |

## Pruebas

| Proyecto | Contenido |
|----------|-----------|
| `TechStore1.Tests/Unitarias` | xUnit + Moq: `ProductoService` (consultar, obtener por ID, registrar, editar, eliminar), `ProductosController`, atributos de autorización, View Component |
| `TechStore1.Tests/Integracion` | `WebApplicationFactory` + EF InMemory: CRUD vía DI, Identity/roles, login real, permisos por rol, reportes, páginas de error |
| `TechStore1.E2ETests` | Playwright (navegador real Edge/Chrome) contra la app en Kestrel: login, menú por rol, consulta, registro/edición, eliminación, acceso denegado, errores 404/500, descargas |

```bash
dotnet test                                   # todo
dotnet test --filter Categoria=Unitaria
dotnet test --filter Categoria=Integracion
dotnet test TechStore1.E2ETests               # E2E (capturas en Evidencias/E2E)
```

Ver el navegador durante las E2E (PowerShell): `$env:HEADED="1"; dotnet test TechStore1.E2ETests`

Reporte de resultados: `dotnet test --logger "html;LogFileName=resultados.html"` (queda en `TestResults/`).
