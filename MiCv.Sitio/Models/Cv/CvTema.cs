namespace MiCv.Sitio.Models.Cv;

public enum CvTema
{
    Minimal = 0,
    Aurora = 1,
    Editorial = 2
}

public static class CvTemaExtensions
{
    private static readonly Dictionary<string, CvTema> Alias = new(StringComparer.OrdinalIgnoreCase)
    {
        ["minimal"] = CvTema.Minimal,
        ["minimo"] = CvTema.Minimal,
        ["aurora"] = CvTema.Aurora,
        ["editorial"] = CvTema.Editorial
    };

    public static CvTema ParseTema(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return CvTema.Minimal;
        return Alias.TryGetValue(raw.Trim(), out var t) ? t : CvTema.Minimal;
    }

    public static string CssClass(this CvTema tema) => tema switch
    {
        CvTema.Aurora => "aurora",
        CvTema.Editorial => "editorial",
        _ => "minimal"
    };

    public static string Titulo(this CvTema tema) => tema switch
    {
        CvTema.Aurora => "Aurora",
        CvTema.Editorial => "Editorial",
        _ => "Minimal"
    };
}
