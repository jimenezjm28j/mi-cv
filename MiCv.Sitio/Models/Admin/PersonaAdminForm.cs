using System.ComponentModel.DataAnnotations;

namespace MiCv.Sitio.Models.Admin;

public class PersonaAdminForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100)]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = "";

    [StringLength(150)]
    [Display(Name = "Título profesional")]
    public string? TituloProfesional { get; set; }

    [Required(ErrorMessage = "El enlace público del CV es obligatorio.")]
    [StringLength(500)]
    [Display(Name = "Enlace público (slug o URL)")]
    public string LinkPerfil { get; set; } = "";

    [StringLength(300)]
    [Display(Name = "Resumen corto")]
    public string? ResumenCorto { get; set; }

    [StringLength(100)]
    [Display(Name = "Ciudad")]
    public string? Ciudad { get; set; }

    [StringLength(100)]
    [Display(Name = "País")]
    public string? Pais { get; set; }

    [StringLength(100)]
    [Display(Name = "Nacionalidad")]
    public string? Nacionalidad { get; set; }

    [Display(Name = "Fecha de nacimiento")]
    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }

    [Display(Name = "Resumen profesional")]
    public string? ResumenProfesional { get; set; }

    [Display(Name = "Objetivo profesional")]
    public string? ObjetivoProfesional { get; set; }

    [StringLength(150)]
    [Display(Name = "Especialidad principal")]
    public string? EspecialidadPrincipal { get; set; }

    [Display(Name = "Años de experiencia")]
    [Range(0, 80)]
    public decimal? AniosExperiencia { get; set; }

    public List<DatoContactoAdminRow> Contactos { get; set; } = new();

    public List<PersonaLinkAdminRow> Links { get; set; } = new();

    public List<ExperienciaAdminRow> Experiencias { get; set; } = new();

    public List<FormacionAdminRow> Formaciones { get; set; } = new();

    public List<CertificacionAdminRow> Certificaciones { get; set; } = new();

    public List<CursoAdminRow> Cursos { get; set; } = new();

    public List<ProyectoAdminRow> Proyectos { get; set; } = new();

    public List<PersonaHabilidadAdminRow> Habilidades { get; set; } = new();

    public List<PersonaIdiomaAdminRow> Idiomas { get; set; } = new();

    public List<PublicacionAdminRow> Publicaciones { get; set; } = new();

    public List<ReconocimientoAdminRow> Reconocimientos { get; set; } = new();

    public List<ReferenciaAdminRow> Referencias { get; set; } = new();

    public static PersonaAdminForm CreateDefaults()
    {
        static IEnumerable<T> Repeat<T>(int n, Func<T> factory) =>
            Enumerable.Range(0, n).Select(_ => factory());

        return new PersonaAdminForm
        {
            Contactos = Repeat(2, () => new DatoContactoAdminRow()).ToList(),
            Links = Repeat(2, () => new PersonaLinkAdminRow()).ToList(),
            Experiencias = Repeat(2, () => new ExperienciaAdminRow { FechaInicio = DateTime.Today }).ToList(),
            Formaciones = Repeat(2, () => new FormacionAdminRow()).ToList(),
            Certificaciones = Repeat(2, () => new CertificacionAdminRow()).ToList(),
            Cursos = Repeat(2, () => new CursoAdminRow()).ToList(),
            Proyectos = Repeat(2, () => new ProyectoAdminRow()).ToList(),
            Habilidades = Repeat(4, () => new PersonaHabilidadAdminRow()).ToList(),
            Idiomas = Repeat(3, () => new PersonaIdiomaAdminRow()).ToList(),
            Publicaciones = Repeat(2, () => new PublicacionAdminRow()).ToList(),
            Reconocimientos = Repeat(2, () => new ReconocimientoAdminRow()).ToList(),
            Referencias = Repeat(2, () => new ReferenciaAdminRow()).ToList()
        };
    }
}
