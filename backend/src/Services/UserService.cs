using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Util;

namespace TodoApp.Services;

public class UserService(
    AppDbContext context,
    JwtUtil jwtUtil,
    ILogger<TodoService> logger) : IUserService
{
    public async Task<UserOutputDto> AuthenticateUserAsync(string username)
    {
        try
        {
            // Fetch user by username
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return new UserOutputDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Generate JWT token
            var token = jwtUtil.GenerateToken(user.Username!, user.Id);
            return new UserOutputDto
            {
                User = user,
                Token = token,
                Message = "User authenticated successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error authenticating user");
            return new UserOutputDto
            {
                Message = "Error authenticating user",
                Success = false
            };
        }
        
    }

    public async Task<UserOutputDto> CreateUserAsync(string email)
    {
        try
        {
            // Check if username already exists
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == email);
            if (existing != null)
            {
                return new UserOutputDto
                {
                    Message = "User already exists",
                    Success = false
                };
            }

            var user = new User
            {
                Username = email
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new UserOutputDto
            {
                User = user,
                Message = "User created successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return new UserOutputDto
            {
                Message = "Error creating user",
                Success = false
            };
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        try
        {
            return await context.Users.FindAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by ID");
            return null;
        }
    }
}