using System.Globalization;
using System.Text.RegularExpressions;

namespace MiCv.Sitio.Models.Cv;

public static class CvFormat
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    private static readonly Regex ImagenUrl = new(@"\.(png|jpe?g|gif|webp|svg)(\?|#|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static bool EsProbableUrlImagen(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        if (url.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            return true;
        return ImagenUrl.IsMatch(url);
    }

    public static string Iniciales(string? nombres, string? apellidos)
    {
        var a = string.IsNullOrWhiteSpace(nombres) ? "" : char.ToUpperInvariant(nombres.Trim()[0]).ToString();
        var b = string.IsNullOrWhiteSpace(apellidos) ? "" : char.ToUpperInvariant(apellidos.Trim()[0]).ToString();
        var s = a + b;
        return string.IsNullOrEmpty(s) ? "?" : s;
    }

    public static string MesAnio(DateTime? fecha) =>
        fecha.HasValue ? fecha.Value.ToString("MMM yyyy", Es) : "";

    public static string Rango(DateTime? inicio, DateTime? fin, bool actualmente)
    {
        var a = MesAnio(inicio);
        if (actualmente || fin is null)
            return string.IsNullOrEmpty(a) ? "Actualidad" : $"{a} — actualidad";
        var b = MesAnio(fin);
        return $"{a} — {b}".Trim();
    }

    public static string UbicacionPersona(string? ciudad, string? pais)
    {
        var parts = new[] { ciudad, pais }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return string.Join(", ", parts);
    }
}
