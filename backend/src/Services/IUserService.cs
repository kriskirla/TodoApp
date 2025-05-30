using Todo.Models;
using TodoApp.Models;

namespace TodoApp.Services;

public interface IUserService
{
    Task<ServiceResult<LoginResponse>> AuthenticateUserAsync(AuthenticateUserRequest request);
    Task<ServiceResult<User>> CreateUserAsync(RegistrationRequest request);
    Task<ServiceResult<User>> GetUserByIdAsync(Guid id);
    Task<ServiceResult<User>> GetUserByEmailAsync(string email);
}
