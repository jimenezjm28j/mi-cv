namespace MiCv.Sitio.Models.Admin;

public class PersonaListaVm
{
    public int Id { get; init; }

    public string Nombres { get; init; } = "";

    public string Apellidos { get; init; } = "";

    public string? LinkPerfil { get; init; }

    public DateTime FechaCreacion { get; init; }

    public DateTime? FechaActualizacion { get; init; }
}
