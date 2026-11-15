using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore1.Models;

namespace TechStore1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuario = await _userManager.FindByNameAsync(model.Usuario);
                if (usuario == null)
                {
                    _logger.LogWarning("Intento de inicio de sesión con usuario inexistente: {Usuario}", model.Usuario);
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                var resultado = await _signInManager.PasswordSignInAsync(usuario, model.Contrasena, isPersistent: false, lockoutOnFailure: false);
                if (resultado.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    _logger.LogInformation("Inicio de sesión exitoso: {Usuario} (roles: {Roles})", usuario.UserName, string.Join(", ", roles));

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                _logger.LogWarning("Contraseña incorrecta para el usuario {Usuario}", model.Usuario);
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el inicio de sesión de {Usuario}", model.Usuario);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión. Intente nuevamente.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var nombre = User.Identity?.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Cierre de sesión: {Usuario}", nombre);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            _logger.LogWarning("Acceso denegado: el usuario {Usuario} intentó acceder a {Ruta}",
                User.Identity?.Name ?? "anónimo", returnUrl);
            Response.StatusCode = StatusCodes.Status403Forbidden;
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
