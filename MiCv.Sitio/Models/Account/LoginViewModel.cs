using System.ComponentModel.DataAnnotations;

namespace MiCv.Sitio.Models.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Indica el correo.")]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "Correo")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Indica la contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
