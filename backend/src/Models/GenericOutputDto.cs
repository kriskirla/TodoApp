namespace TodoApp.Models;

public class GenericOutputDto
{
    public Guid? Id { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; } = true;
}