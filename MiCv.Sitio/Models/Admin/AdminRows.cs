using System.ComponentModel.DataAnnotations;

namespace MiCv.Sitio.Models.Admin;

public class DatoContactoAdminRow
{
    [StringLength(50)] public string TipoContacto { get; set; } = "";
    [StringLength(250)] public string Valor { get; set; } = "";
    public bool EsPrincipal { get; set; }
    public bool EsPublico { get; set; } = true;
}

public class PersonaLinkAdminRow
{
    [StringLength(50)] public string TipoLink { get; set; } = "";
    [StringLength(500)] public string Url { get; set; } = "";
    [StringLength(150)] public string? TextoVisible { get; set; }
    public bool EsPrincipal { get; set; }
    public int Orden { get; set; }
}

public class ExperienciaAdminRow
{
    [StringLength(200)] public string? Empresa { get; set; }
    [StringLength(150)] public string? Cargo { get; set; }
    [StringLength(150)] public string? Ubicacion { get; set; }
    [StringLength(50)] public string? Modalidad { get; set; }
    [StringLength(50)] public string? TipoContrato { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaInicio { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaFin { get; set; }
    public bool Actualmente { get; set; }
    public string? Descripcion { get; set; }
    public string? Logros { get; set; }
    public int? Orden { get; set; }
    /// <summary>Una responsabilidad por línea.</summary>
    public string? ResponsabilidadesLineas { get; set; }
}

public class FormacionAdminRow
{
    [StringLength(200)] public string? Institucion { get; set; }
    [StringLength(200)] public string? Titulo { get; set; }
    [StringLength(150)] public string? AreaEstudio { get; set; }
    [StringLength(100)] public string? NivelAcademico { get; set; }
    [StringLength(150)] public string? Ubicacion { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaInicio { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaFin { get; set; }
    public bool Actualmente { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }
}

public class CertificacionAdminRow
{
    [StringLength(200)] public string? Nombre { get; set; }
    [StringLength(200)] public string? EntidadEmisora { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaEmision { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaExpiracion { get; set; }
    [StringLength(150)] public string? CodigoCredencial { get; set; }
    [StringLength(500)] public string? LinkCredencial { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }
}

public class CursoAdminRow
{
    [StringLength(200)] public string? Nombre { get; set; }
    [StringLength(200)] public string? Institucion { get; set; }
    [StringLength(150)] public string? Plataforma { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaInicio { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaFin { get; set; }
    public decimal? Horas { get; set; }
    [StringLength(500)] public string? LinkCertificado { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }
}

public class ProyectoAdminRow
{
    [StringLength(200)] public string? Nombre { get; set; }
    [StringLength(150)] public string? Rol { get; set; }
    [StringLength(200)] public string? ClienteEmpresa { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaInicio { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaFin { get; set; }
    public bool Actualmente { get; set; }
    public string? Descripcion { get; set; }
    [StringLength(500)] public string? LinkRepositorio { get; set; }
    [StringLength(500)] public string? LinkDemo { get; set; }
    public int? Orden { get; set; }
}

public class PersonaHabilidadAdminRow
{
    public int HabilidadId { get; set; }
    [StringLength(50)] public string? Nivel { get; set; }
    public decimal? AniosExperiencia { get; set; }
    public bool EsPrincipal { get; set; }
    public int Orden { get; set; }
}

public class PersonaIdiomaAdminRow
{
    public int IdiomaId { get; set; }
    [StringLength(50)] public string? NivelGeneral { get; set; }
    [StringLength(50)] public string? NivelLectura { get; set; }
    [StringLength(50)] public string? NivelEscritura { get; set; }
    [StringLength(50)] public string? NivelConversacion { get; set; }
    [StringLength(150)] public string? Certificacion { get; set; }
    public int Orden { get; set; }
}

public class PublicacionAdminRow
{
    [StringLength(250)] public string? Titulo { get; set; }
    [StringLength(200)] public string? Medio { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaPublicacion { get; set; }
    [StringLength(500)] public string? LinkPublicacion { get; set; }
    public string? Descripcion { get; set; }
}

public class ReconocimientoAdminRow
{
    [StringLength(200)] public string? Nombre { get; set; }
    [StringLength(200)] public string? Entidad { get; set; }
    [DataType(DataType.Date)] public DateTime? Fecha { get; set; }
    public string? Descripcion { get; set; }
}

public class ReferenciaAdminRow
{
    [StringLength(150)] public string? Nombre { get; set; }
    [StringLength(150)] public string? Cargo { get; set; }
    [StringLength(200)] public string? Empresa { get; set; }
    [StringLength(150)] public string? Email { get; set; }
    [StringLength(50)] public string? Telefono { get; set; }
    [StringLength(100)] public string? Relacion { get; set; }
    public bool DisponiblePreviaSolicitud { get; set; }
}
