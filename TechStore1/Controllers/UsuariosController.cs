using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechStore1.Authorization;
using TechStore1.Models;

namespace TechStore1.Controllers
{
    // Usuarios y roles (solo Administrador)
    [Authorize(Roles = AppRoles.Administrador)]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            ILogger<UsuariosController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
            var modelo = new List<UsuarioRolViewModel>();
            foreach (var usuario in usuarios)
            {
                modelo.Add(new UsuarioRolViewModel
                {
                    Id = usuario.Id,
                    Usuario = usuario.UserName ?? string.Empty,
                    Email = usuario.Email,
                    Roles = await _userManager.GetRolesAsync(usuario)
                });
            }

            ViewBag.Roles = await ObtenerRolesAsync();
            return View(modelo);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new SelectList(await ObtenerRolesAsync());
            return View(new CrearUsuarioViewModel { Rol = AppRoles.Cliente });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearUsuarioViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Rol) && !await _roleManager.RoleExistsAsync(model.Rol))
            {
                ModelState.AddModelError(nameof(model.Rol), "El rol seleccionado no existe.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(await ObtenerRolesAsync(), model.Rol);
                return View(model);
            }

            try
            {
                var usuario = new IdentityUser { UserName = model.Usuario, Email = model.Email };
                var resultado = await _userManager.CreateAsync(usuario, model.Contrasena);
                if (!resultado.Succeeded)
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    ViewBag.Roles = new SelectList(await ObtenerRolesAsync(), model.Rol);
                    return View(model);
                }

                await _userManager.AddToRoleAsync(usuario, model.Rol);
                _logger.LogInformation("El administrador {Admin} creó el usuario {Usuario} con rol {Rol}",
                    User.Identity?.Name, model.Usuario, model.Rol);
                TempData["Mensaje"] = $"Usuario '{model.Usuario}' creado con el rol {model.Rol}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario {Usuario}", model.Usuario);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el usuario.");
                ViewBag.Roles = new SelectList(await ObtenerRolesAsync(), model.Rol);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(string id, string rol)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                if (!await _roleManager.RoleExistsAsync(rol))
                {
                    TempData["Error"] = "El rol seleccionado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                if (usuario.UserName == User.Identity?.Name && rol != AppRoles.Administrador)
                {
                    TempData["Error"] = "No puedes quitarte el rol de Administrador a ti mismo.";
                    return RedirectToAction(nameof(Index));
                }

                var rolesActuales = await _userManager.GetRolesAsync(usuario);
                await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
                await _userManager.AddToRoleAsync(usuario, rol);

                _logger.LogInformation("El administrador {Admin} cambió el rol de {Usuario}: {RolesAnteriores} -> {RolNuevo}",
                    User.Identity?.Name, usuario.UserName, string.Join(", ", rolesActuales), rol);
                TempData["Mensaje"] = $"Rol de '{usuario.UserName}' actualizado a {rol}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el rol del usuario {UsuarioId}", id);
                TempData["Error"] = "Ocurrió un error al cambiar el rol.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<string>> ObtenerRolesAsync() =>
            await _roleManager.Roles.Select(r => r.Name!).OrderBy(n => n).ToListAsync();
    }
}
