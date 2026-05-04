using System.ComponentModel.DataAnnotations;

namespace MiCv.Sitio.Models.Account;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "Correo")]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = "";

    [StringLength(50)]
    [Display(Name = "Nombre para mostrar")]
    public string? NombreMostrar { get; set; }
}
