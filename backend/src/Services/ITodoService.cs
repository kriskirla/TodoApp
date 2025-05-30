using TodoApp.Enums;
using TodoApp.Models;

namespace TodoApp.Services;

public interface ITodoService
{
    Task<ServiceResult<TodoList>> CreateListAsync(TodoList listId);
    Task<ServiceResult<TodoList>> GetListAsync(Guid listId);
    Task<ServiceResult<TodoList>> UpdateListAsync(Guid listId, TodoList update);
    Task<ServiceResult<TodoList>> DeleteListAsync(Guid listId);
    Task<ServiceResult<TodoList>> AddItemToListAsync(Guid listId, TodoItemForm itemForm);
    Task<ServiceResult<TodoList>> DeleteItemFromListAsync(Guid listId, Guid itemId);
    Task<ServiceResult<TodoList>> ShareListAsync(Guid listId, ShareRequest request);
    Task<ServiceResult<TodoList>> UnshareListAsync(Guid listId, Guid userId);
    Task<ServiceResult<IEnumerable<TodoList>>> GetAllListByUserIdAsync();
    Task<ServiceResult<TodoList>> FilterListItemsAsync(Guid listId, AttributeType filter, string? key);
    Task<ServiceResult<TodoList>> SortListItemsAsync(Guid listId, AttributeType sort, OrderType? order);
    Task<ServiceResult<TodoList>> SortFilteredListItemsAsync(Guid listId, AttributeType filter, string? key, AttributeType sort, OrderType? order);
}