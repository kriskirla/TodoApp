using TodoApp.Models;

namespace TodoApp.Services;

public interface ITodoService
{
    Task<TodoListOutputDto> CreateListAsync(TodoList listId);
    Task<TodoList?> GetListAsync(Guid listId);
    Task<TodoListOutputDto> UpdateListAsync(TodoList list, TodoList update);
    Task<TodoListOutputDto> DeleteListAsync(TodoList list);
    Task<TodoListOutputDto> AddItemToListAsync(TodoList list, TodoItemForm itemForm);
    Task<TodoListOutputDto> DeleteItemFromListAsync(TodoList list, TodoItem item);
    Task<TodoListOutputDto> ShareListAsync(TodoList list, ShareRequest request);
    Task<TodoListOutputDto> UnshareListAsync(TodoList list, TodoListShare share, ShareRequest request);
    Task<IEnumerable<TodoList>?> GetAllListByUserIdAsync(Guid userId);
}