using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class RegistrationRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;
}