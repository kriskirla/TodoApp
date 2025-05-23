namespace TodoApp.Models;

public class TodoListOutputDto
{
    public TodoList? List { get; set; }
    public TodoItem? Item { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; } = true;
}