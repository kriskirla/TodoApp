using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Enums;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/[controller]")]
public class TodoController(ITodoService todoService) : BaseController
{
    /// <summary>
    /// This API creates the list given TodoList information
    /// </summary>
    /// <param name="list"></param>
    /// <returns>The TodoList with unique ID and owner ID associated</returns>
    [HttpPost("list/create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateList([FromBody] TodoList list)
    {
        return FromServiceResult(await todoService.CreateListAsync(list));
    }

    /// <summary>
    /// This API retrieves a specific todo list by ID, with optional filtering and sorting of its items.
    /// </summary>
    /// <param name="listId">The ID of the todo list.</param>
    /// <param name="filter">Optional: Attribute type to filter items. (Name = 0, Description = 1, DueDate = 2, Status = 3, Priority = 4)</param>
    /// <param name="key">Optional: Filter key.</param>
    /// <param name="sort">Optional: Attribute type to sort items. (Name = 0, Description = 1, DueDate = 2, Status = 3, Priority = 4)</param>
    /// <param name="order">Optional: Sort order. (Descending = 0, Ascending = 1)</param>
    /// <returns>The requested todo list and optionally filtered/sorted items.</returns>
    [HttpGet("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetList(
        Guid listId,
        [FromQuery] AttributeType? filter,
        [FromQuery] string? key,
        [FromQuery] AttributeType? sort,
        [FromQuery] OrderType? order)
    {
        if (filter != null && sort != null)
        {
            return FromServiceResult(await todoService.SortFilteredListItemsAsync(listId, filter.Value, key, sort.Value, order));
        }
        else if (filter != null)
        {
            return FromServiceResult(await todoService.FilterListItemsAsync(listId, filter.Value, key));
        }
        else if (sort != null)
        {
            return FromServiceResult(await todoService.SortListItemsAsync(listId, sort.Value, order));
        }
        return FromServiceResult(await todoService.GetListAsync(listId));
    }

    /// <summary>
    /// This API updates an existing todo list.
    /// </summary>
    /// <param name="listId">The ID of the list to update.</param>
    /// <param name="update">The updated list information.</param>
    /// <returns>The updated todo list.</returns>
    [HttpPut("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateList(Guid listId, [FromBody] TodoList update)
    {
        return FromServiceResult(await todoService.UpdateListAsync(listId, update));
    }

    /// <summary>
    /// This API deletes a todo list by ID.
    /// </summary>
    /// <param name="listId">The ID of the list to delete.</param>
    /// <returns>The deleted todo list.</returns>
    [HttpDelete("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteList(Guid listId)
    {
        return FromServiceResult(await todoService.DeleteListAsync(listId));
    }

    /// <summary>
    /// This API adds a new item to the specified todo list.
    /// </summary>
    /// <param name="listId">The ID of the list to add the item to.</param>
    /// <param name="itemForm">The form containing the item data.</param>
    /// <returns>The updated todo list with the new item.</returns>
    [HttpPost("item/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid listId, [FromForm] TodoItemForm itemForm)
    {
        return FromServiceResult(await todoService.AddItemToListAsync(listId, itemForm));
    }

    /// <summary>
    /// This API deletes a specific item from a todo list.
    /// </summary>
    /// <param name="listId">The ID of the list containing the item.</param>
    /// <param name="itemId">The ID of the item to delete.</param>
    /// <returns>The updated todo list without the deleted item.</returns>
    [HttpDelete("item/{listId}/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(Guid listId, Guid itemId)
    {
        return FromServiceResult(await todoService.DeleteItemFromListAsync(listId, itemId));
    }

    /// <summary>
    /// This API shares a todo list with another user.
    /// </summary>
    /// <param name="listId">The ID of the list to share.</param>
    /// <param name="request">The request containing the user ID and permission.</param>
    /// <returns>The shared todo list with updated sharing info.</returns>
    [HttpPost("list/share/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShareList(Guid listId, [FromBody] ShareRequest request)
    {
        return FromServiceResult(await todoService.ShareListAsync(listId, request));
    }

    /// <summary>
    /// This API revokes a user's access to a shared todo list.
    /// </summary>
    /// <param name="listId">The ID of the list to unshare.</param>
    /// <param name="userId">The ID of the user to unshare from the list.</param>
    /// <returns>The updated todo list without the user access.</returns>
    [HttpDelete("list/unshare/{listId}/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnshareList(Guid listId, Guid userId)
    {
        return FromServiceResult(await todoService.UnshareListAsync(listId, userId));
    }

    /// <summary>
    /// This API retrieves all todo lists owned or shared with the current user.
    /// </summary>
    /// <returns>A list of todo lists associated with the current user.</returns>
    [HttpGet("list/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TodoList>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllListByUserId()
    {
        return FromServiceResult(await todoService.GetAllListByUserIdAsync());
    }
}
