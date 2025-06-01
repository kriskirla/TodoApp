using Microsoft.EntityFrameworkCore;
using Todo.Models;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Util;

namespace TodoApp.Services;

public class UserService(
    AppDbContext context,
    JwtUtil jwtUtil,
    ILogger<TodoService> logger) : IUserService
{
    public async Task<ServiceResult<LoginResponse>> AuthenticateUserAsync(AuthenticateUserRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return ServiceResult<LoginResponse>.BadRequest("Email is required.");
            }

            // Fetch user by email
            var result = await GetUserByEmailAsync(request.Email);
            var user = result.Data;
            if (user == null || user.Username == null)
            {
                return ServiceResult<LoginResponse>.NotFound(result.Error!.Message);
            }

            // Generate JWT token
            var token = jwtUtil.GenerateToken(user.Username, user.Id);
            return ServiceResult<LoginResponse>.Success(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error authenticating user {}", request.Email);
            return ServiceResult<LoginResponse>.Unknown("Error authenticating user");
        }
    }

    public async Task<ServiceResult<User>> CreateUserAsync(RegistrationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return ServiceResult<User>.BadRequest("Email is required");
            }

            var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Email);
            if (existing != null)
            {
                return ServiceResult<User>.BadRequest("User already exists");
            }

            var user = new User
            {
                Username = request.Email
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return ServiceResult<User>.Success(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user {email}", request.Email);
            return ServiceResult<User>.Unknown("Error creating user");
        }
    }

    public async Task<ServiceResult<User>> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return ServiceResult<User>.NotFound("User not found");
            }

            return ServiceResult<User>.Success(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by ID {userId}", id);
            return ServiceResult<User>.Unknown("Error fetching user by ID");
        }
    }

    public async Task<ServiceResult<User>> GetUserByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                return ServiceResult<User>.BadRequest("Email is required");
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == email);

            if (user == null)
            {
                return ServiceResult<User>.NotFound("User not found");
            }

            return ServiceResult<User>.Success(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by email {email}", email);
            return ServiceResult<User>.Unknown("Error fetching user by email");
        }
    }
}