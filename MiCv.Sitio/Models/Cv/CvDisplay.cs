using MiCv.Data.Databases.MiCv;

namespace MiCv.Sitio.Models.Cv;

public static class CvDisplay
{
    public static IEnumerable<DatoContacto> ContactosPublicos(Persona p) =>
        p.DatoContacto
            .Where(c => c.EsPublico)
            .OrderByDescending(c => c.EsPrincipal)
            .ThenBy(c => c.TipoContacto);

    public static IEnumerable<PersonaLink> LinksOrdenados(Persona p) =>
        p.PersonaLink.OrderBy(l => l.Orden).ThenBy(l => l.TipoLink);

    public static IEnumerable<PersonaHabilidad> HabilidadesOrdenadas(Persona p) =>
        p.PersonaHabilidad
            .OrderByDescending(h => h.EsPrincipal)
            .ThenBy(h => h.Orden)
            .ThenBy(h => h.Habilidad?.Categoria)
            .ThenBy(h => h.Habilidad?.Nombre);

    public static IEnumerable<PersonaIdioma> IdiomasOrdenados(Persona p) =>
        p.PersonaIdioma.OrderBy(i => i.Idioma?.Nombre);

    public static IEnumerable<ExperienciaLaboral> ExperienciasOrdenadas(Persona p) =>
        p.ExperienciaLaboral
            .OrderByDescending(e => e.Actualmente)
            .ThenByDescending(e => e.FechaInicio)
            .ThenBy(e => e.Orden ?? int.MaxValue);

    public static IEnumerable<FormacionAcademica> FormacionOrdenada(Persona p) =>
        p.FormacionAcademica
            .OrderByDescending(f => f.Actualmente)
            .ThenByDescending(f => f.FechaFin ?? f.FechaInicio)
            .ThenBy(f => f.Orden ?? int.MaxValue);

    public static IEnumerable<Certificacion> CertificacionesOrdenadas(Persona p) =>
        p.Certificacion
            .OrderByDescending(c => c.FechaEmision)
            .ThenBy(c => c.Orden ?? int.MaxValue);

    public static IEnumerable<Curso> CursosOrdenados(Persona p) =>
        p.Curso
            .OrderByDescending(c => c.FechaFin ?? c.FechaInicio)
            .ThenBy(c => c.Orden ?? int.MaxValue);

    public static IEnumerable<Proyecto> ProyectosOrdenados(Persona p) =>
        p.Proyecto
            .OrderByDescending(r => r.Actualmente)
            .ThenByDescending(r => r.FechaFin ?? r.FechaInicio)
            .ThenBy(r => r.Orden ?? int.MaxValue);

    public static IEnumerable<Publicacion> PublicacionesOrdenadas(Persona p) =>
        p.Publicacion.OrderByDescending(x => x.FechaPublicacion);

    public static IEnumerable<Reconocimiento> ReconocimientosOrdenados(Persona p) =>
        p.Reconocimiento.OrderByDescending(x => x.Fecha);

    public static IEnumerable<Referencia> ReferenciasOrdenadas(Persona p) =>
        p.Referencia.OrderBy(r => r.Nombre);

    public static IEnumerable<ExperienciaResponsabilidad> ResponsabilidadesOrdenadas(
        ExperienciaLaboral e
    ) => e.ExperienciaResponsabilidad.OrderBy(r => r.Orden);

    public static IEnumerable<ProyectoHabilidad> ProyectoSkillsOrdenadas(Proyecto proyecto) =>
        proyecto.ProyectoHabilidad.OrderBy(ph => ph.Habilidad?.Nombre);
}
