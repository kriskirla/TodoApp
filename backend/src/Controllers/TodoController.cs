using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApp.Enums;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/[controller]")]
public class TodoController(
    ITodoService todoService,
    IUserService userService) : ControllerBase
{
    [HttpPost("list/create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoListOutputDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateList([FromBody] TodoList list)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            var claimsInfo = string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}"));
            return Unauthorized($"Invalid or missing user ID in token. {claimsInfo}");
        }

        list.OwnerId = userId.Value;
        return Ok(await todoService.CreateListAsync(list));
    }

    [HttpGet("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetList(Guid listId)
    {
        var list = await todoService.GetListAsync(listId);

        if (list == null)
        {
            return NotFound("The todo list cannot be found");
        }
        else if (!IsOwner(list) && !IsSharedViewOnly(list) && !IsSharedEditPermission(list))
        {
            return Forbid("You are not the owner or lack view/edit permission to this list");
        }
        return Ok(list);
    }

    [HttpPut("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoListOutputDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateList(Guid listId, [FromBody] TodoList update)
    {
        var list = await todoService.GetListAsync(listId);

        if (list == null)
        {
            return NotFound("The todo list cannot be found");
        }

        if (!IsOwner(list) && !IsSharedEditPermission(list))
        {
            return Forbid("You are not the owner or lack edit permission to this list");
        }

        return Ok(await todoService.UpdateListAsync(list, update));
    }

    [HttpDelete("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoListOutputDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteList(Guid listId)
    {
        var list = await todoService.GetListAsync(listId);
        if (list == null)
        {
            return NotFound("The todo list cannot be found");
        }

        if (!IsOwner(list))
        {
            return Forbid("You are not the owner of this list");
        }
        return Ok(await todoService.DeleteListAsync(list));
    }

    [HttpPost("item/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoListOutputDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid listId, [FromForm] TodoItemForm itemForm)
    {
        var list = await todoService.GetListAsync(listId);
        if (list == null)
        {
            return NotFound("The todo list cannot be found");
        }
        else if (!IsOwner(list) && !IsSharedEditPermission(list))
        {
            return Forbid("You are not the owner or lack edit permission to this list");
        }
        return Ok(await todoService.AddItemToListAsync(list, itemForm));
    }

    [HttpDelete("item/{listId}/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoListOutputDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(Guid listId, Guid itemId)
    {
        var list = await todoService.GetListAsync(listId);
        if (list == null)
        {
            return NotFound("The todo list cannot be found");
        }
        else if (!IsOwner(list) && !IsSharedEditPermission(list))
        {
            return Forbid("You are not the owner or lack edit permission to this list");
        }
        var item = list.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            return NotFound("The todo item cannot be found");
        }
        return Ok(await todoService.DeleteItemFromListAsync(list, item));
    }

    [HttpPost("share/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoListOutputDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShareList(Guid listId, [FromBody] ShareRequest request)
    {
        var list = await todoService.GetListAsync(listId);
        if (list == null)
        {
            return NotFound("The todo list cannot be found");
        }
        else if (!IsOwner(list))
        {
            return Forbid("You are not the owner of this list");
        }
        else if (request.UserId == Guid.Empty)
        {
            return BadRequest("User ID is required");
        }
        else if (await userService.GetUserByIdAsync(request.UserId) == null)
        {
            return NotFound("User not found");
        }
        return Ok(await todoService.ShareListAsync(list, request));
    }

    [HttpPost("unshare/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoListOutputDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnshareList(Guid listId, [FromBody] ShareRequest request)
    {
        var list = await todoService.GetListAsync(listId);
        if (list == null)
        {
            return NotFound("The todo list cannot be found");
        }
        else if (!IsOwner(list))
        {
            return Forbid("You are not the owner of this list");
        }
        else if (request.UserId == Guid.Empty)
        {
            return BadRequest("User ID is required");
        }

        var user = await userService.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            return NotFound("User not found");
        }
        // Check if user is shared
        var share = list.SharedWith.FirstOrDefault(u => u.SharedWithUserId == user.Id);
        if (share == null)
        {
            return NotFound("User is not shared with this list");
        }
        return Ok(await todoService.UnshareListAsync(list, share, request));
    }

    #region Private Methods
    private Guid? GetCurrentUserId()
    {
        return Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ? userId : null;
    }

    private bool IsOwner(TodoList list)
    {
        var userId = GetCurrentUserId();
        return list != null && userId.HasValue && list.OwnerId == userId.Value;
    }

    private bool IsSharedEditPermission(TodoList list)
    {
        var userId = GetCurrentUserId();
        return list != null
        && userId.HasValue
        && list.SharedWith.Any(
            s => s.SharedWithUserId == userId.Value
            && s.Permission == PermissionType.Edit);
    }

    private bool IsSharedViewOnly(TodoList list)
    {
        var userId = GetCurrentUserId();
        return list != null
        && userId.HasValue
        && list.SharedWith.Any(
            s => s.SharedWithUserId == userId.Value
            && s.Permission == PermissionType.View);
    }
    #endregion
}
