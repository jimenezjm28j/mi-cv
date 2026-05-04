using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCv.Data.Databases;
using MiCv.Data.Databases.MiCv;
using MiCv.Sitio.Models.Cv;
using MiCv.Sitio.Security;

namespace MiCv.Sitio.Controllers;

[Route("/Cv")]
public class CvController(MiCvContext db) : Controller
{
    [Route("{link}")]
    public async Task<IActionResult> Index(string link, [FromQuery(Name = "tema")] string? tema)
    {
        var persona = await BuscarPersonaAsync(link);
        if (persona is null)
            return NotFound();

        var uid = User.GetUsuarioId();
        var puedeAdministrar =
            uid.HasValue && persona.UsuarioId.HasValue && persona.UsuarioId.Value == uid.Value;

        var vm = new CvPageViewModel
        {
            Persona = persona,
            LinkSlug = link.Trim(),
            Tema = CvTemaExtensions.ParseTema(tema),
            PuedeAdministrar = puedeAdministrar,
            PersonaId = persona.Id
        };

        return View(vm);
    }

    private async Task<Persona?> BuscarPersonaAsync(string linkRaw)
    {
        var link = linkRaw.Trim().Trim('/');
        if (string.IsNullOrEmpty(link))
            return null;

        IQueryable<Persona> q = db.Persona.AsNoTracking();

        q = q
            .Include(p => p.PerfilProfesional)
            .Include(p => p.DatoContacto)
            .Include(p => p.PersonaLink)
            .Include(p => p.PersonaHabilidad).ThenInclude(ph => ph.Habilidad)
            .Include(p => p.PersonaIdioma).ThenInclude(pi => pi.Idioma)
            .Include(p => p.ExperienciaLaboral).ThenInclude(e => e.ExperienciaResponsabilidad)
            .Include(p => p.FormacionAcademica)
            .Include(p => p.Certificacion)
            .Include(p => p.Curso)
            .Include(p => p.Proyecto).ThenInclude(pr => pr.ProyectoHabilidad).ThenInclude(ph => ph.Habilidad)
            .Include(p => p.Publicacion)
            .Include(p => p.Reconocimiento)
            .Include(p => p.Referencia);

        if (int.TryParse(link, out var id))
            return await q.FirstOrDefaultAsync(p => p.Id == id);

        return await q.FirstOrDefaultAsync(p =>
                p.LinkPerfil == link
            //p.LinkPerfil != null
            //&& (
            //    p.LinkPerfil == link
            //    || p.LinkPerfil.EndsWith("/" + link, StringComparison.OrdinalIgnoreCase)
            //    || p.LinkPerfil.EndsWith("\\" + link, StringComparison.OrdinalIgnoreCase)
            //)
            );
    }
}
