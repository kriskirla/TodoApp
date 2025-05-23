using Microsoft.AspNetCore.Mvc;
using Todo.Models;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class AuthenticationController(IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginOutputDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AuthenticateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest("Email is required.");
        }

        var result = await userService.AuthenticateUserAsync(request.Email);

        if (!result.Success || result.Token == null)
        {
            return Unauthorized("Invalid Email.");
        }

        return Ok(new LoginOutputDto
        {
            Token = result.Token
        });
    }

    [HttpPost("user/create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserOutputDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateUser(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email is required.");
        }
        return Ok(await userService.CreateUserAsync(email));
    }

    [HttpGet("user/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound("User not found.");
        }
        return Ok(user);
    }

    [HttpGet("user/email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email is required.");
        }

        var user = await userService.GetUserByEmailAsync(email);

        if (user == null)
        {
            return NotFound("User not found.");
        }
        return Ok(user);
    }
}
