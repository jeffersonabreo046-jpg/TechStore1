using System.Net;
using System.Text.RegularExpressions;

namespace TechStore1.Tests.Infraestructura;

public static partial class HttpClientLoginExtensions
{
    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();

    public static async Task<string> ObtenerTokenAntiforgeryAsync(this HttpClient cliente, string url)
    {
        var html = await cliente.GetStringAsync(url);
        var coincidencia = TokenRegex().Match(html);
        if (!coincidencia.Success)
        {
            throw new InvalidOperationException($"No se encontró el token antiforgery en {url}");
        }
        return WebUtility.HtmlDecode(coincidencia.Groups[1].Value);
    }

    // Inicia sesión con el formulario de login
    public static async Task<HttpResponseMessage> IniciarSesionAsync(this HttpClient cliente, string usuario, string contrasena)
    {
        var token = await cliente.ObtenerTokenAntiforgeryAsync("/Account/Login");
        var formulario = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Usuario"] = usuario,
            ["Contrasena"] = contrasena,
            ["__RequestVerificationToken"] = token
        });
        return await cliente.PostAsync("/Account/Login", formulario);
    }
}
