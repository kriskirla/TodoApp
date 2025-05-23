namespace Todo.Models;

public class LoginOutputDto
{
    public string? Token { get; set; }
    public DateTime Expiration { get; set; }
}