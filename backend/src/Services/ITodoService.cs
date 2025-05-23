using TodoApp.Models;

namespace TodoApp.Services;

public interface ITodoService
{
    Task<TodoList> CreateListAsync(TodoList listId);
    Task<TodoList?> GetListAsync(Guid listId);
    Task<TodoList?> UpdateListAsync(TodoList list, TodoList update);
    Task<GenericOutputDto> DeleteListAsync(TodoList list);
    Task<GenericOutputDto> AddItemToListAsync(TodoList list, TodoItemForm itemForm);
    Task<GenericOutputDto> ShareListAsync(TodoList list, ShareRequest request);
    Task<GenericOutputDto> UnshareListAsync(TodoList list, TodoListShare share, ShareRequest request);
}