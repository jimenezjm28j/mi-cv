using System.ComponentModel.DataAnnotations;

namespace MiCv.Sitio.Models.Account;

public class CreatePasswordHashViewModel
{
    [Required(ErrorMessage = "Escribe la contraseña.")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = "";

    /// <summary>Resultado generado (solo lectura en la vista).</summary>
    public string? HashGenerado { get; set; }
}
