namespace TodoApp.Models;

public class UserOutputDto
{
    public User? User { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; } = true;
}
