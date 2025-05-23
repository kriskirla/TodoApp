using TodoApp.Models;

namespace TodoApp.Services;

public interface IUserService
{
    Task<(bool Success, string Token)> AuthenticateUserAsync(string username);
    Task<User?> CreateUserAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
}
