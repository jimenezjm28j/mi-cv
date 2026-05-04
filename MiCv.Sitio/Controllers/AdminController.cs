using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiCv.Data.Databases;
using MiCv.Data.Databases.MiCv;
using MiCv.Sitio.Models.Admin;
using MiCv.Sitio.Security;

namespace MiCv.Sitio.Controllers;

[Authorize]
[Route("Admin")]
public class AdminController(MiCvContext db) : Controller
{
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var uid = User.GetUsuarioId();
        if (uid is null)
            return Unauthorized();

        var lista = await db.Persona.AsNoTracking()
            .Where(p => p.UsuarioId == uid)
            .OrderByDescending(p => p.FechaActualizacion ?? p.FechaCreacion)
            .Select(p => new PersonaListaVm
            {
                Id = p.Id,
                Nombres = p.Nombres,
                Apellidos = p.Apellidos,
                LinkPerfil = p.LinkPerfil,
                FechaCreacion = p.FechaCreacion,
                FechaActualizacion = p.FechaActualizacion
            })
            .ToListAsync(ct);

        return View(lista);
    }

    [Route("Cv/Nuevo")]
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await LoadLookupsAsync(ct);
        return View(PersonaAdminForm.CreateDefaults());
    }

    [Route("Cv/Nuevo")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonaAdminForm vm, CancellationToken ct)
    {
        var uid = User.GetUsuarioId();
        if (uid is null)
            return Unauthorized();

        PadRepeatableLists(vm);
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(ct);
            return View(vm);
        }

        var persona = new Persona
        {
            Nombres = vm.Nombres.Trim(),
            Apellidos = vm.Apellidos.Trim(),
            TituloProfesional = vm.TituloProfesional?.Trim(),
            LinkPerfil = vm.LinkPerfil.Trim(),
            ResumenCorto = vm.ResumenCorto?.Trim(),
            Ciudad = vm.Ciudad?.Trim(),
            Pais = vm.Pais?.Trim(),
            Nacionalidad = vm.Nacionalidad?.Trim(),
            FechaNacimiento = vm.FechaNacimiento,
            UsuarioId = uid,
            FechaCreacion = DateTime.UtcNow
        };

        db.Persona.Add(persona);
        await db.SaveChangesAsync(ct);

        var perfil = new PerfilProfesional
        {
            PersonaId = persona.Id,
            Resumen = string.IsNullOrWhiteSpace(vm.ResumenProfesional)
                ? "\u00a0"
                : vm.ResumenProfesional.Trim(),
            ObjetivoProfesional = vm.ObjetivoProfesional?.Trim(),
            EspecialidadPrincipal = vm.EspecialidadPrincipal?.Trim(),
            AniosExperiencia = vm.AniosExperiencia
        };
        db.PerfilProfesional.Add(perfil);
        await db.SaveChangesAsync(ct);

        InsertPersonaChildren(persona.Id, vm);
        await db.SaveChangesAsync(ct);

        TempData["Msg"] = "CV creado.";
        return RedirectToAction(nameof(Edit), new { id = persona.Id });
    }

    [Route("Cv/Editar/{id:int}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var uid = User.GetUsuarioId();
        if (uid is null)
            return Unauthorized();

        var persona = await LoadPersonaFullAsync(id, uid.Value, ct);
        if (persona is null)
            return NotFound();

        var vm = MapPersonaToForm(persona);
        PadRepeatableLists(vm);
        await LoadLookupsAsync(ct);
        return View(vm);
    }

    [Route("Cv/Editar/{id:int}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PersonaAdminForm vm, CancellationToken ct)
    {
        var uid = User.GetUsuarioId();
        if (uid is null)
            return Unauthorized();

        if (id != vm.Id)
            return BadRequest();

        PadRepeatableLists(vm);
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(ct);
            return View(vm);
        }

        var persona = await db.Persona
            .Include(p => p.PerfilProfesional)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == uid, ct);

        if (persona is null)
            return NotFound();

        persona.Nombres = vm.Nombres.Trim();
        persona.Apellidos = vm.Apellidos.Trim();
        persona.TituloProfesional = vm.TituloProfesional?.Trim();
        persona.LinkPerfil = vm.LinkPerfil.Trim();
        persona.ResumenCorto = vm.ResumenCorto?.Trim();
        persona.Ciudad = vm.Ciudad?.Trim();
        persona.Pais = vm.Pais?.Trim();
        persona.Nacionalidad = vm.Nacionalidad?.Trim();
        persona.FechaNacimiento = vm.FechaNacimiento;
        persona.FechaActualizacion = DateTime.UtcNow;

        if (persona.PerfilProfesional is null)
        {
            persona.PerfilProfesional = new PerfilProfesional { PersonaId = persona.Id, Resumen = "\u00a0" };
            db.PerfilProfesional.Add(persona.PerfilProfesional);
        }

        persona.PerfilProfesional.Resumen = string.IsNullOrWhiteSpace(vm.ResumenProfesional)
            ? "\u00a0"
            : vm.ResumenProfesional.Trim();
        persona.PerfilProfesional.ObjetivoProfesional = vm.ObjetivoProfesional?.Trim();
        persona.PerfilProfesional.EspecialidadPrincipal = vm.EspecialidadPrincipal?.Trim();
        persona.PerfilProfesional.AniosExperiencia = vm.AniosExperiencia;

        await ClearPersonaChildrenAsync(id, ct);
        InsertPersonaChildren(id, vm);

        await db.SaveChangesAsync(ct);

        TempData["Msg"] = "Cambios guardados.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        var habilidades = await db.Habilidad.AsNoTracking()
            .OrderBy(h => h.Categoria).ThenBy(h => h.Nombre).ToListAsync(ct);
        ViewBag.HabilidadesList = habilidades
            .Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(h.Categoria)
                    ? h.Nombre
                    : $"{h.Nombre} · {h.Categoria}"
            })
            .ToList();

        var idiomas = await db.Idioma.AsNoTracking().OrderBy(i => i.Nombre).ToListAsync(ct);
        ViewBag.IdiomasList = idiomas
            .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Nombre })
            .ToList();
    }

    private async Task<Persona?> LoadPersonaFullAsync(int id, int usuarioId, CancellationToken ct) =>
        await db.Persona
            .AsSplitQuery()
            .Include(p => p.PerfilProfesional)
            .Include(p => p.DatoContacto)
            .Include(p => p.PersonaLink)
            .Include(p => p.ExperienciaLaboral).ThenInclude(e => e.ExperienciaResponsabilidad)
            .Include(p => p.FormacionAcademica)
            .Include(p => p.Certificacion)
            .Include(p => p.Curso)
            .Include(p => p.Proyecto)
            .Include(p => p.PersonaHabilidad)
            .Include(p => p.PersonaIdioma)
            .Include(p => p.Publicacion)
            .Include(p => p.Reconocimiento)
            .Include(p => p.Referencia)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId, ct);

    private static void PadRepeatableLists(PersonaAdminForm vm, int min = 2, int maxHabilidades = 12)
    {
        vm.Contactos ??= new List<DatoContactoAdminRow>();
        vm.Links ??= new List<PersonaLinkAdminRow>();
        vm.Experiencias ??= new List<ExperienciaAdminRow>();
        vm.Formaciones ??= new List<FormacionAdminRow>();
        vm.Certificaciones ??= new List<CertificacionAdminRow>();
        vm.Cursos ??= new List<CursoAdminRow>();
        vm.Proyectos ??= new List<ProyectoAdminRow>();
        vm.Habilidades ??= new List<PersonaHabilidadAdminRow>();
        vm.Idiomas ??= new List<PersonaIdiomaAdminRow>();
        vm.Publicaciones ??= new List<PublicacionAdminRow>();
        vm.Reconocimientos ??= new List<ReconocimientoAdminRow>();
        vm.Referencias ??= new List<ReferenciaAdminRow>();

        static void Pad<T>(List<T> list, int minimum, Func<T> factory)
        {
            while (list.Count < minimum)
                list.Add(factory());
        }

        Pad(vm.Contactos, min, () => new DatoContactoAdminRow());
        Pad(vm.Links, min, () => new PersonaLinkAdminRow());
        Pad(vm.Experiencias, min, () => new ExperienciaAdminRow { FechaInicio = DateTime.Today });
        Pad(vm.Formaciones, min, () => new FormacionAdminRow());
        Pad(vm.Certificaciones, min, () => new CertificacionAdminRow());
        Pad(vm.Cursos, min, () => new CursoAdminRow());
        Pad(vm.Proyectos, min, () => new ProyectoAdminRow());
        Pad(vm.Habilidades, Math.Min(min + 2, maxHabilidades), () => new PersonaHabilidadAdminRow());
        Pad(vm.Idiomas, min + 1, () => new PersonaIdiomaAdminRow());
        Pad(vm.Publicaciones, min, () => new PublicacionAdminRow());
        Pad(vm.Reconocimientos, min, () => new ReconocimientoAdminRow());
        Pad(vm.Referencias, min, () => new ReferenciaAdminRow());
    }

    private static PersonaAdminForm MapPersonaToForm(Persona p)
    {
        var vm = new PersonaAdminForm
        {
            Id = p.Id,
            Nombres = p.Nombres,
            Apellidos = p.Apellidos,
            TituloProfesional = p.TituloProfesional,
            LinkPerfil = p.LinkPerfil ?? "",
            ResumenCorto = p.ResumenCorto,
            Ciudad = p.Ciudad,
            Pais = p.Pais,
            Nacionalidad = p.Nacionalidad,
            FechaNacimiento = p.FechaNacimiento,
            ResumenProfesional = p.PerfilProfesional?.Resumen?.Trim() == "\u00a0"
                ? ""
                : p.PerfilProfesional?.Resumen,
            ObjetivoProfesional = p.PerfilProfesional?.ObjetivoProfesional,
            EspecialidadPrincipal = p.PerfilProfesional?.EspecialidadPrincipal,
            AniosExperiencia = p.PerfilProfesional?.AniosExperiencia,
            Contactos = p.DatoContacto.OrderByDescending(c => c.EsPrincipal).ThenBy(c => c.Id).Select(c =>
                    new DatoContactoAdminRow
                    {
                        TipoContacto = c.TipoContacto,
                        Valor = c.Valor,
                        EsPrincipal = c.EsPrincipal,
                        EsPublico = c.EsPublico
                    })
                .ToList(),
            Links = p.PersonaLink.OrderBy(l => l.Orden).ThenBy(l => l.Id).Select(l =>
                    new PersonaLinkAdminRow
                    {
                        TipoLink = l.TipoLink,
                        Url = l.Url,
                        TextoVisible = l.TextoVisible,
                        EsPrincipal = l.EsPrincipal,
                        Orden = l.Orden
                    })
                .ToList(),
            Experiencias = CvOrderingExperiencias(p).Select(e =>
                new ExperienciaAdminRow
                {
                    Empresa = e.Empresa,
                    Cargo = e.Cargo,
                    Ubicacion = e.Ubicacion,
                    Modalidad = e.Modalidad,
                    TipoContrato = e.TipoContrato,
                    FechaInicio = e.FechaInicio,
                    FechaFin = e.FechaFin,
                    Actualmente = e.Actualmente,
                    Descripcion = e.Descripcion,
                    Logros = e.Logros,
                    Orden = e.Orden,
                    ResponsabilidadesLineas = string.Join(Environment.NewLine,
                        e.ExperienciaResponsabilidad.OrderBy(r => r.Orden).Select(r => r.Descripcion))
                }).ToList(),
            Formaciones = p.FormacionAcademica.OrderByDescending(f => f.Actualmente)
                .ThenByDescending(f => f.FechaFin ?? f.FechaInicio).ThenBy(f => f.Orden ?? int.MaxValue).Select(f =>
                    new FormacionAdminRow
                    {
                        Institucion = f.Institucion,
                        Titulo = f.Titulo,
                        AreaEstudio = f.AreaEstudio,
                        NivelAcademico = f.NivelAcademico,
                        Ubicacion = f.Ubicacion,
                        FechaInicio = f.FechaInicio,
                        FechaFin = f.FechaFin,
                        Actualmente = f.Actualmente,
                        Descripcion = f.Descripcion,
                        Orden = f.Orden
                    }).ToList(),
            Certificaciones = p.Certificacion.OrderByDescending(c => c.FechaEmision).ThenBy(c => c.Orden ?? int.MaxValue)
                .Select(c => new CertificacionAdminRow
                {
                    Nombre = c.Nombre,
                    EntidadEmisora = c.EntidadEmisora,
                    FechaEmision = c.FechaEmision,
                    FechaExpiracion = c.FechaExpiracion,
                    CodigoCredencial = c.CodigoCredencial,
                    LinkCredencial = c.LinkCredencial,
                    Descripcion = c.Descripcion,
                    Orden = c.Orden
                }).ToList(),
            Cursos = p.Curso.OrderByDescending(c => c.FechaFin ?? c.FechaInicio).ThenBy(c => c.Orden ?? int.MaxValue)
                .Select(c => new CursoAdminRow
                {
                    Nombre = c.Nombre,
                    Institucion = c.Institucion,
                    Plataforma = c.Plataforma,
                    FechaInicio = c.FechaInicio,
                    FechaFin = c.FechaFin,
                    Horas = c.Horas,
                    LinkCertificado = c.LinkCertificado,
                    Descripcion = c.Descripcion,
                    Orden = c.Orden
                }).ToList(),
            Proyectos = p.Proyecto.OrderByDescending(r => r.Actualmente)
                .ThenByDescending(r => r.FechaFin ?? r.FechaInicio).ThenBy(r => r.Orden ?? int.MaxValue)
                .Select(pr => new ProyectoAdminRow
                {
                    Nombre = pr.Nombre,
                    Rol = pr.Rol,
                    ClienteEmpresa = pr.ClienteEmpresa,
                    FechaInicio = pr.FechaInicio,
                    FechaFin = pr.FechaFin,
                    Actualmente = pr.Actualmente,
                    Descripcion = pr.Descripcion,
                    LinkRepositorio = pr.LinkRepositorio,
                    LinkDemo = pr.LinkDemo,
                    Orden = pr.Orden
                }).ToList(),
            Habilidades = p.PersonaHabilidad.OrderByDescending(ph => ph.EsPrincipal).ThenBy(ph => ph.Orden)
                .Select(ph => new PersonaHabilidadAdminRow
                {
                    HabilidadId = ph.HabilidadId,
                    Nivel = ph.Nivel,
                    AniosExperiencia = ph.AniosExperiencia,
                    EsPrincipal = ph.EsPrincipal,
                    Orden = ph.Orden
                }).ToList(),
            Idiomas = p.PersonaIdioma.OrderBy(pi => pi.Orden).ThenBy(pi => pi.Id).Select(pi =>
                    new PersonaIdiomaAdminRow
                    {
                        IdiomaId = pi.IdiomaId,
                        NivelGeneral = pi.NivelGeneral,
                        NivelLectura = pi.NivelLectura,
                        NivelEscritura = pi.NivelEscritura,
                        NivelConversacion = pi.NivelConversacion,
                        Certificacion = pi.Certificacion,
                        Orden = pi.Orden
                    })
                .ToList(),
            Publicaciones = p.Publicacion.OrderByDescending(x => x.FechaPublicacion).Select(pub =>
                    new PublicacionAdminRow
                    {
                        Titulo = pub.Titulo,
                        Medio = pub.Medio,
                        FechaPublicacion = pub.FechaPublicacion,
                        LinkPublicacion = pub.LinkPublicacion,
                        Descripcion = pub.Descripcion
                    })
                .ToList(),
            Reconocimientos = p.Reconocimiento.OrderByDescending(r => r.Fecha).Select(r =>
                    new ReconocimientoAdminRow
                    {
                        Nombre = r.Nombre,
                        Entidad = r.Entidad,
                        Fecha = r.Fecha,
                        Descripcion = r.Descripcion
                    })
                .ToList(),
            Referencias = p.Referencia.OrderBy(r => r.Nombre).Select(r =>
                    new ReferenciaAdminRow
                    {
                        Nombre = r.Nombre,
                        Cargo = r.Cargo,
                        Empresa = r.Empresa,
                        Email = r.Email,
                        Telefono = r.Telefono,
                        Relacion = r.Relacion,
                        DisponiblePreviaSolicitud = r.DisponiblePreviaSolicitud
                    })
                .ToList()
        };

        return vm;
    }

    private static IEnumerable<ExperienciaLaboral> CvOrderingExperiencias(Persona p) =>
        p.ExperienciaLaboral.OrderByDescending(e => e.Actualmente).ThenByDescending(e => e.FechaInicio)
            .ThenBy(e => e.Orden ?? int.MaxValue);

    private async Task ClearPersonaChildrenAsync(int personaId, CancellationToken ct)
    {
        var expIds = await db.ExperienciaLaboral.AsNoTracking()
            .Where(e => e.PersonaId == personaId).Select(e => e.Id).ToListAsync(ct);
        if (expIds.Count > 0)
            await db.ExperienciaResponsabilidad.Where(r => expIds.Contains(r.ExperienciaLaboralId))
                .ExecuteDeleteAsync(ct);

        await db.ExperienciaLaboral.Where(e => e.PersonaId == personaId).ExecuteDeleteAsync(ct);

        var projIds = await db.Proyecto.AsNoTracking()
            .Where(p => p.PersonaId == personaId).Select(p => p.Id).ToListAsync(ct);
        if (projIds.Count > 0)
            await db.ProyectoHabilidad.Where(ph => projIds.Contains(ph.ProyectoId)).ExecuteDeleteAsync(ct);

        await db.Proyecto.Where(p => p.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.PersonaHabilidad.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.PersonaIdioma.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.PersonaLink.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.DatoContacto.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.FormacionAcademica.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.Certificacion.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.Curso.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.Publicacion.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.Reconocimiento.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
        await db.Referencia.Where(x => x.PersonaId == personaId).ExecuteDeleteAsync(ct);
    }

    private static IEnumerable<string> SplitResponsabilidadLines(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            yield break;
        foreach (var line in raw.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var t = line.Trim();
            if (t.Length == 0)
                continue;
            yield return t.Length <= 500 ? t : t[..500];
        }
    }

    private void InsertPersonaChildren(int personaId, PersonaAdminForm vm)
    {
        foreach (var c in vm.Contactos.Where(x =>
                     !string.IsNullOrWhiteSpace(x.TipoContacto) && !string.IsNullOrWhiteSpace(x.Valor)))
            db.DatoContacto.Add(new DatoContacto
            {
                PersonaId = personaId,
                TipoContacto = c.TipoContacto.Trim(),
                Valor = c.Valor.Trim(),
                EsPrincipal = c.EsPrincipal,
                EsPublico = c.EsPublico
            });

        foreach (var l in vm.Links.Where(x =>
                     !string.IsNullOrWhiteSpace(x.TipoLink) && !string.IsNullOrWhiteSpace(x.Url)))
            db.PersonaLink.Add(new PersonaLink
            {
                PersonaId = personaId,
                TipoLink = l.TipoLink.Trim(),
                Url = l.Url.Trim(),
                TextoVisible = l.TextoVisible?.Trim(),
                EsPrincipal = l.EsPrincipal,
                Orden = l.Orden
            });

        foreach (var row in vm.Experiencias.Where(x =>
                     !string.IsNullOrWhiteSpace(x.Empresa) && !string.IsNullOrWhiteSpace(x.Cargo)))
        {
            var fi = row.FechaInicio ?? DateTime.UtcNow.Date;
            var exp = new ExperienciaLaboral
            {
                PersonaId = personaId,
                Empresa = row.Empresa!.Trim(),
                Cargo = row.Cargo!.Trim(),
                Ubicacion = row.Ubicacion?.Trim(),
                Modalidad = row.Modalidad?.Trim(),
                TipoContrato = row.TipoContrato?.Trim(),
                FechaInicio = fi,
                FechaFin = row.FechaFin,
                Actualmente = row.Actualmente,
                Descripcion = row.Descripcion?.Trim(),
                Logros = row.Logros?.Trim(),
                Orden = row.Orden
            };

            var o = 0;
            foreach (var desc in SplitResponsabilidadLines(row.ResponsabilidadesLineas))
                exp.ExperienciaResponsabilidad.Add(new ExperienciaResponsabilidad
                    { Descripcion = desc, Orden = ++o });

            db.ExperienciaLaboral.Add(exp);
        }

        foreach (var f in vm.Formaciones.Where(x =>
                     !string.IsNullOrWhiteSpace(x.Institucion) && !string.IsNullOrWhiteSpace(x.Titulo)))
            db.FormacionAcademica.Add(new FormacionAcademica
            {
                PersonaId = personaId,
                Institucion = f.Institucion!.Trim(),
                Titulo = f.Titulo!.Trim(),
                AreaEstudio = f.AreaEstudio?.Trim(),
                NivelAcademico = f.NivelAcademico?.Trim(),
                Ubicacion = f.Ubicacion?.Trim(),
                FechaInicio = f.FechaInicio,
                FechaFin = f.FechaFin,
                Actualmente = f.Actualmente,
                Descripcion = f.Descripcion?.Trim(),
                Orden = f.Orden
            });

        foreach (var c in vm.Certificaciones.Where(x =>
                     !string.IsNullOrWhiteSpace(x.Nombre) && !string.IsNullOrWhiteSpace(x.EntidadEmisora)))
            db.Certificacion.Add(new Certificacion
            {
                PersonaId = personaId,
                Nombre = c.Nombre!.Trim(),
                EntidadEmisora = c.EntidadEmisora!.Trim(),
                FechaEmision = c.FechaEmision,
                FechaExpiracion = c.FechaExpiracion,
                CodigoCredencial = c.CodigoCredencial?.Trim(),
                LinkCredencial = c.LinkCredencial?.Trim(),
                Descripcion = c.Descripcion?.Trim(),
                Orden = c.Orden
            });

        foreach (var c in vm.Cursos.Where(x => !string.IsNullOrWhiteSpace(x.Nombre)))
            db.Curso.Add(new Curso
            {
                PersonaId = personaId,
                Nombre = c.Nombre!.Trim(),
                Institucion = c.Institucion?.Trim(),
                Plataforma = c.Plataforma?.Trim(),
                FechaInicio = c.FechaInicio,
                FechaFin = c.FechaFin,
                Horas = c.Horas,
                LinkCertificado = c.LinkCertificado?.Trim(),
                Descripcion = c.Descripcion?.Trim(),
                Orden = c.Orden
            });

        foreach (var pr in vm.Proyectos.Where(x => !string.IsNullOrWhiteSpace(x.Nombre)))
            db.Proyecto.Add(new Proyecto
            {
                PersonaId = personaId,
                Nombre = pr.Nombre!.Trim(),
                Rol = pr.Rol?.Trim(),
                ClienteEmpresa = pr.ClienteEmpresa?.Trim(),
                FechaInicio = pr.FechaInicio,
                FechaFin = pr.FechaFin,
                Actualmente = pr.Actualmente,
                Descripcion = pr.Descripcion?.Trim(),
                LinkRepositorio = pr.LinkRepositorio?.Trim(),
                LinkDemo = pr.LinkDemo?.Trim(),
                Orden = pr.Orden
            });

        foreach (var h in vm.Habilidades.Where(x => x.HabilidadId > 0))
            db.PersonaHabilidad.Add(new PersonaHabilidad
            {
                PersonaId = personaId,
                HabilidadId = h.HabilidadId,
                Nivel = h.Nivel?.Trim(),
                AniosExperiencia = h.AniosExperiencia,
                EsPrincipal = h.EsPrincipal,
                Orden = h.Orden
            });

        foreach (var i in vm.Idiomas.Where(x => x.IdiomaId > 0))
            db.PersonaIdioma.Add(new PersonaIdioma
            {
                PersonaId = personaId,
                IdiomaId = i.IdiomaId,
                NivelGeneral = i.NivelGeneral?.Trim(),
                NivelLectura = i.NivelLectura?.Trim(),
                NivelEscritura = i.NivelEscritura?.Trim(),
                NivelConversacion = i.NivelConversacion?.Trim(),
                Certificacion = i.Certificacion?.Trim(),
                Orden = i.Orden
            });

        foreach (var pub in vm.Publicaciones.Where(x => !string.IsNullOrWhiteSpace(x.Titulo)))
            db.Publicacion.Add(new Publicacion
            {
                PersonaId = personaId,
                Titulo = pub.Titulo!.Trim(),
                Medio = pub.Medio?.Trim(),
                FechaPublicacion = pub.FechaPublicacion,
                LinkPublicacion = pub.LinkPublicacion?.Trim(),
                Descripcion = pub.Descripcion?.Trim()
            });

        foreach (var rec in vm.Reconocimientos.Where(x => !string.IsNullOrWhiteSpace(x.Nombre)))
            db.Reconocimiento.Add(new Reconocimiento
            {
                PersonaId = personaId,
                Nombre = rec.Nombre!.Trim(),
                Entidad = rec.Entidad?.Trim(),
                Fecha = rec.Fecha,
                Descripcion = rec.Descripcion?.Trim()
            });

        foreach (var r in vm.Referencias.Where(x => !string.IsNullOrWhiteSpace(x.Nombre)))
            db.Referencia.Add(new Referencia
            {
                PersonaId = personaId,
                Nombre = r.Nombre!.Trim(),
                Cargo = r.Cargo?.Trim(),
                Empresa = r.Empresa?.Trim(),
                Email = r.Email?.Trim(),
                Telefono = r.Telefono?.Trim(),
                Relacion = r.Relacion?.Trim(),
                DisponiblePreviaSolicitud = r.DisponiblePreviaSolicitud
            });
    }
}
