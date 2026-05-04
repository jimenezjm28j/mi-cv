using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCv.Data.Databases;
using MiCv.Data.Databases.MiCv;
using MiCv.Sitio.Models.Account;
using MiCv.Sitio.Security;

namespace MiCv.Sitio.Controllers;

public class AccountController(MiCvContext db, IWebHostEnvironment env) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ShowRegister = env.IsDevelopment();
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, CancellationToken ct)
    {
        ViewBag.ShowRegister = env.IsDevelopment();
        if (!ModelState.IsValid)
            return View(vm);

        var usuario = await db.Usuario.AsNoTracking().FirstOrDefaultAsync(
            u => u.Correo == vm.Email.Trim() && u.Activo,
            ct);

        var passwordOk = usuario != null
             && PasswordHashing.MatchesSha256Hex(vm.Password, usuario.Contrasenna);

        if (usuario is null || !passwordOk)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Name, usuario.Nombre ?? usuario.Correo)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true });

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Admin");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (!env.IsDevelopment())
            return NotFound();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm, CancellationToken ct)
    {
        if (!env.IsDevelopment())
            return NotFound();

        if (!ModelState.IsValid)
            return View(vm);

        var email = vm.Email.Trim();
        if (await db.Usuario.AnyAsync(u => u.Correo == email, ct))
        {
            ModelState.AddModelError(nameof(vm.Email), "Ese correo ya está registrado.");
            return View(vm);
        }

        var nombre = NormalizarNombreUsuario(email, vm.NombreMostrar);
        var usuario = new Usuario
        {
            Correo = email,
            Contrasenna = PasswordHashing.CreateHash(vm.Password.Trim()),
            Nombre = nombre,
            Activo = true
        };

        db.Usuario.Add(usuario);
        await db.SaveChangesAsync(ct);

        TempData["Msg"] = "Usuario creado. Ya puedes iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult CreatePasswordHash()
    {
        if (!env.IsDevelopment())
            return NotFound();
        return View(new CreatePasswordHashViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult CreatePasswordHash(CreatePasswordHashViewModel vm)
    {
        if (!env.IsDevelopment())
            return NotFound();

        if (!ModelState.IsValid)
            return View(vm);

        vm.HashGenerado = PasswordHashing.CreateHash(vm.Password.Trim());
        vm.Password = "";
        vm.ConfirmPassword = "";
        ModelState.Clear();
        return View(vm);
    }

    /// <summary>La columna Nombre admite máximo 50 caracteres.</summary>
    private static string NormalizarNombreUsuario(string correo, string? nombreMostrar)
    {
        var raw = string.IsNullOrWhiteSpace(nombreMostrar)
            ? correo.Split('@')[0]
            : nombreMostrar.Trim();
        return raw.Length <= 50 ? raw : raw[..50];
    }
}
