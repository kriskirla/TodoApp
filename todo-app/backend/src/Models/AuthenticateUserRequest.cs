using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class AuthenticateUserRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;
}