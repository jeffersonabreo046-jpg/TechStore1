namespace TechStore1.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public int CodigoEstado { get; set; } = 500;

        public string? Ruta { get; set; }

        public string Titulo => CodigoEstado switch
        {
            404 => "Página no encontrada",
            403 => "Acceso denegado",
            400 => "Solicitud incorrecta",
            _ => "Ocurrió un error inesperado"
        };

        public string Mensaje => CodigoEstado switch
        {
            404 => "El recurso que buscas no existe o fue eliminado.",
            403 => "No tienes permisos para realizar esta acción.",
            400 => "La solicitud no es válida.",
            _ => "Lo sentimos, ocurrió un problema al procesar tu solicitud. El error fue registrado y será revisado."
        };
    }
}
