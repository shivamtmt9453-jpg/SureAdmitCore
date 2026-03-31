using System.ComponentModel.DataAnnotations;

namespace SureAdmitCore.Models
{
    public class LoginViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; }=string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    }
}
