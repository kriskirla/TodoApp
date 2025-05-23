using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class TodoItemForm
{
    [Required]
    public string? Description { get; set; }

    public IFormFile? Media { get; set; }
}
