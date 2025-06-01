using TodoApp.Enums;

namespace TodoApp.Models;

public class TodoItemForm
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public StatusType? Status { get; set; }
    public PriorityType? Priority { get; set; }
    public IFormFile? Media { get; set; }
}
