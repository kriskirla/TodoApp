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
    [HttpPost("list/create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateList([FromBody] TodoList list)
    {
        return FromServiceResult(await todoService.CreateListAsync(list));
    }

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

    [HttpPut("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateList(Guid listId, [FromBody] TodoList update)
    {
        return FromServiceResult(await todoService.UpdateListAsync(listId, update));
    }

    [HttpDelete("list/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteList(Guid listId)
    {
        return FromServiceResult(await todoService.DeleteListAsync(listId));
    }

    [HttpPost("item/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(Guid listId, [FromForm] TodoItemForm itemForm)
    {
        return FromServiceResult(await todoService.AddItemToListAsync(listId, itemForm));
    }

    [HttpDelete("item/{listId}/{itemId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(Guid listId, Guid itemId)
    {
        return FromServiceResult(await todoService.DeleteItemFromListAsync(listId, itemId));
    }

    [HttpPost("list/share/{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShareList(Guid listId, [FromBody] ShareRequest request)
    {
        return FromServiceResult(await todoService.ShareListAsync(listId, request));
    }

    [HttpDelete("list/unshare/{listId}/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TodoList))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnshareList(Guid listId, Guid userId)
    {
        return FromServiceResult(await todoService.UnshareListAsync(listId, userId));
    }

    [HttpGet("list/user")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TodoList>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllListByUserId()
    {
        return FromServiceResult(await todoService.GetAllListByUserIdAsync());
    }
}
