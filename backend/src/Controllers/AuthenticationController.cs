using Microsoft.AspNetCore.Mvc;
using Todo.Models;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class AuthenticationController(IUserService userService) : BaseController
{
    /// <summary>
    /// Authenticates a user and returns a JWT token upon successful login.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <returns>A JWT token and user info if authentication is successful.</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AuthenticateUserRequest request)
    {
        return FromServiceResult(await userService.AuthenticateUserAsync(request));
    }

    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <param name="request">The registration request with user details.</param>
    /// <returns>The created user object.</returns>
    [HttpPost("user/create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateUser([FromBody] RegistrationRequest request)
    {
        return FromServiceResult(await userService.CreateUserAsync(request));
    }

    /// <summary>
    /// Retrieves a user by their unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user object if found.</returns>
    [HttpGet("user/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        return FromServiceResult(await userService.GetUserByIdAsync(id));
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>The user object if a match is found.</returns>
    [HttpGet("user/email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        return FromServiceResult(await userService.GetUserByEmailAsync(email));
    }
}
