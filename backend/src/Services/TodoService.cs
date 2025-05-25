using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Enums;
using TodoApp.Models;
using TodoApp.Util;

namespace TodoApp.Services;

public class TodoService(
    AppDbContext context,
    IHubContext<TodoHub> hubContext,
    ILogger<TodoService> logger) : ITodoService
{
    public async Task<TodoListOutputDto> CreateListAsync(TodoList list)
    {
        try
        {
            list.Id = Guid.NewGuid();
            context.TodoLists.Add(list);
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ListCreated");
            return new TodoListOutputDto
            {
                List = list,
                Message = "Todo list created successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create TodoList");
            return new TodoListOutputDto
            {
                Message = "Failed to create todo list",
                Success = false
            };
        }
    }

    public async Task<TodoList?> GetListAsync(Guid listId)
    {
        try
        {
            // This checks if eneity is tracked before making a query to the database
            var list = await context.TodoLists.FindAsync(listId);

            if (list != null)
            {
                // Manually load navigation properties if not already loaded
                if (!context.Entry(list).Collection(l => l.Items).IsLoaded)
                {
                    await context.Entry(list).Collection(l => l.Items).LoadAsync();
                }

                if (!context.Entry(list).Collection(l => l.SharedWith).IsLoaded)
                {
                    await context.Entry(list).Collection(l => l.SharedWith).LoadAsync();
                }
            }
            return list;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch TodoList with ID {ListId}", listId);
            return null;
        }
    }

    public async Task<TodoListOutputDto> UpdateListAsync(TodoList list, TodoList update)
    {
        try
        {
            list.Title = update.Title;
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ListUpdated", list);
            return new TodoListOutputDto
            {
                List = list,
                Message = "Todo list updated successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update TodoList with ID {ListId}", list.Id);
            return new TodoListOutputDto
            {
                Message = "Failed to update todo list",
                Success = false
            };
        }
    }

    public async Task<TodoListOutputDto> DeleteListAsync(TodoList list)
    {
        try
        {
            // Delete items within the list before deleting list
            var items = context.TodoItems.Where(i => i.TodoListId == list.Id).ToList();

            foreach (var item in items)
            {
                var result = await DeleteItemFromListAsync(list, item);
                if (!result.Success)
                {
                    logger.LogWarning("Failed to delete item {ItemId} from list {ListId}", item.Id, list.Id);
                    return new TodoListOutputDto
                    {
                        Message = "Failed to delete todo list because items could not be deleted",
                        Success = false
                    };
                }
            }

            context.TodoLists.Remove(list);
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ListDeleted", list);
            return new TodoListOutputDto
            {
                List = list,
                Message = "Todo list deleted successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete TodoList with ID {ListId}", list.Id);
            return new TodoListOutputDto
            {
                Message = "Failed to delete todo list",
                Success = false
            };
        }
    }

    public async Task<TodoListOutputDto> AddItemToListAsync(TodoList list, TodoItemForm itemForm)
    {
        try
        {
            var item = new TodoItem
            {
                Id = Guid.NewGuid(),
                Description = itemForm.Description,
                TodoListId = list.Id
            };

            if (itemForm.Media != null)
            {
                var extension = Path.GetExtension(itemForm.Media.FileName).ToLower();
                // For simplicity, we are only allowing mp4 and mov for video
                // In real world scenario, you would want to validate the file type and size
                // Also, extension could be spoofed, so using a library to check the file type would be better
                var mediaType = extension is ".mp4" or ".mov" ? MediaType.Video : MediaType.Image;
                var fileName = $"{Guid.NewGuid()}{extension}";
                var mediaDirectory = Path.Combine(Directory.GetCurrentDirectory(), "media");

                var filePath = Path.Combine(mediaDirectory, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await itemForm.Media.CopyToAsync(stream);
                item.MediaUrl = $"/media/{fileName}";
                item.MediaType = mediaType;
            }

            context.TodoItems.Add(item);
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ItemAdded", item);
            return new TodoListOutputDto
            {
                Item = item,
                Message = $"Item {item.Id} added successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add item to TodoList with ID {ListId}", list.Id);
            return new TodoListOutputDto
            {
                Message = "Failed to add item to todo list",
                Success = false
            };
        }
    }

    public async Task<TodoListOutputDto> DeleteItemFromListAsync(TodoList list, TodoItem item)
    {
        try
        {
            // Delete associated media file if it exists
            if (!string.IsNullOrEmpty(item.MediaUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), item.MediaUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            context.TodoItems.Remove(item);
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ItemDeleted", item);
            return new TodoListOutputDto
            {
                Item = item,
                Message = $"Item {item.Id} deleted successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete item from TodoList with ID {ListId}", list.Id);
            return new TodoListOutputDto
            {
                Message = "Failed to delete item from todo list",
                Success = false
            };
        }
    }

    public async Task<TodoListOutputDto> ShareListAsync(TodoList list, ShareRequest request)
    {
        try
        {
            // Check if the user is already shared
            if (list.SharedWith.Any(s => s.SharedWithUserId == request.UserId))
            {
                return new TodoListOutputDto
                {
                    Message = $"List already shared with user {request.UserId}",
                    Success = false
                };
            }

            var share = new TodoListShare
            {
                Id = Guid.NewGuid(),
                TodoListId = list.Id,
                SharedWithUserId = request.UserId,
                Permission = request.Permission
            };

            context.TodoListShares.Add(share);
            await context.SaveChangesAsync();
            await hubContext.Clients.User(request.UserId.ToString()).SendAsync("ListShared", list);
            return new TodoListOutputDto
            {
                List = list,
                Message = $"List {list.Id} shared with {request.UserId} successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to share TodoList with ID {ListId} with user {UserId}", list.Id, request.UserId);
            return new TodoListOutputDto
            {
                Message = $"Failed to share list {list.Id} with {request.UserId}",
                Success = false
            };
        }
    }

    public async Task<TodoListOutputDto> UnshareListAsync(TodoList list, TodoListShare share, ShareRequest request)
    {
        try
        {
            context.TodoListShares.Remove(share);
            await context.SaveChangesAsync();
            await hubContext.Clients.User(request.UserId.ToString()).SendAsync("ListUnshared", list);
            return new TodoListOutputDto
            {
                List = list,
                Message = $"List {list.Id} unshared with {request.UserId} successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unshare TodoList with ID {ListId} from user {UserId}", list.Id, request.UserId);
            return new TodoListOutputDto
            {
                Message = $"Failed to unshare list {list.Id} from {request.UserId}",
                Success = false
            };
        }
    }

    public async Task<IEnumerable<TodoList>> GetAllListByUserIdAsync(Guid userId)
    {
        try
        {
            var lists = await context.TodoLists
                .Where(l => l.OwnerId == userId || l.SharedWith.Any(s => s.SharedWithUserId == userId))
                .ToListAsync();

            return lists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch TodoLists for user {UserId}", userId);
            return [];
        }
    }
}