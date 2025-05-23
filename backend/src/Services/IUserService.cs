using TodoApp.Models;

namespace TodoApp.Services;

public interface IUserService
{
    Task<UserOutputDto> AuthenticateUserAsync(string username);
    Task<UserOutputDto> CreateUserAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
}
