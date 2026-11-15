namespace TechStore1.Authorization
{
    // Roles del sistema
    public static class AppRoles
    {
        public const string Administrador = "Administrador";
        public const string Vendedor = "Vendedor";
        public const string Cliente = "Cliente";

        // Roles combinados para [Authorize(Roles = ...)]
        public const string AdministradorOVendedor = Administrador + "," + Vendedor;

        // Políticas de autorización
        public const string GestionProductos = "GestionProductos";
        public const string EliminacionProductos = "EliminacionProductos";
        public const string ConsultaReportes = "ConsultaReportes";
        public const string AdministracionGeneral = "AdministracionGeneral";

        public static readonly string[] Todos = { Administrador, Vendedor, Cliente };
    }
}
