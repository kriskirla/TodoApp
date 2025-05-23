using System.ComponentModel.DataAnnotations;
using TodoApp.Enums;

namespace TodoApp.Models;

public class ShareRequest
{
    [Required]
    public Guid UserId { get; set; } = Guid.Empty;
    public PermissionType Permission { get; set; }
}