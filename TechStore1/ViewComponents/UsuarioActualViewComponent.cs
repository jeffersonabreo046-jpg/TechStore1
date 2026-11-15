using Microsoft.AspNetCore.Mvc;
using TechStore1.Authorization;

namespace TechStore1.ViewComponents
{
    // Muestra el usuario y su rol
    public class UsuarioActualViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var rol = AppRoles.Todos.FirstOrDefault(r => UserClaimsPrincipal.IsInRole(r)) ?? "Sin rol";
            var color = rol switch
            {
                AppRoles.Administrador => "bg-danger",
                AppRoles.Vendedor => "bg-warning text-dark",
                AppRoles.Cliente => "bg-info text-dark",
                _ => "bg-secondary"
            };

            return View((Nombre: UserClaimsPrincipal.Identity?.Name ?? "", Rol: rol, Color: color));
        }
    }
}
