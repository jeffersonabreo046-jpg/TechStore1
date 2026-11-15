namespace TechStore1.Authorization
{
    public static class AppRoles
    {
        public const string Administrador = "Administrador";
        public const string Vendedor = "Vendedor";
        public const string Cliente = "Cliente";

        public const string GestionProductos = "GestionProductos";
        public const string EliminacionProductos = "EliminacionProductos";
        public const string ConsultaReportes = "ConsultaReportes";
        public const string AdministracionGeneral = "AdministracionGeneral";

        public static readonly string[] Todos = { Administrador, Vendedor, Cliente };
    }
}
