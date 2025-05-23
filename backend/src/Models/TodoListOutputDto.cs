namespace TodoApp.Models;

public class TodoListOutputDto
{
    public Guid? Id { get; set; }
    public TodoList? List { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; } = true;
}