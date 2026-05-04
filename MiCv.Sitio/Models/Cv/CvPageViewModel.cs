using MiCv.Data.Databases.MiCv;

namespace MiCv.Sitio.Models.Cv;

public class CvPageViewModel
{
    public required Persona Persona { get; init; }

    public required string LinkSlug { get; init; }

    public CvTema Tema { get; init; }

    public bool PuedeAdministrar { get; init; }

    public int PersonaId { get; init; }
}
