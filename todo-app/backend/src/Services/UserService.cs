using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Util;

namespace TodoApp.Services;

public class UserService(
    AppDbContext context,
    JwtUtil jwtUtil) : IUserService
{
    public async Task<(bool, string)> AuthenticateUserAsync(string username)
    {
        // Fetch user by username
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return (false, "User not found");
        }

        // Generate JWT token
        var token = jwtUtil.GenerateToken(user.Username!, user.Id);
        return (true, token);
    }

    public async Task<User?> CreateUserAsync(string email)
    {
        // Check if username already exists
        var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == email);
        if (existing != null)
        {
            return null;
        }

        var user = new User
        {
            Username = email
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await context.Users.FindAsync(id);
    }
}