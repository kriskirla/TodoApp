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
    public async Task<TodoList> CreateListAsync(TodoList list)
    {
        list.Id = Guid.NewGuid();
        context.TodoLists.Add(list);
        await context.SaveChangesAsync();
        logger.LogInformation("Created new TodoList with ID {ListId}", list.Id);
        return list;
    }

    public async Task<TodoList?> GetListAsync(Guid listId)
    {
        // This checks if eneity is tracked before making a query to the database
        logger.LogInformation("Fetching TodoList with ID {ListId}", listId);
        var todoList = await context.TodoLists.FindAsync(listId);

        if (todoList != null)
        {
            // Manually load navigation properties if not already loaded
            if (!context.Entry(todoList).Collection(l => l.Items).IsLoaded)
            {
                await context.Entry(todoList).Collection(l => l.Items).LoadAsync();
            }

            if (!context.Entry(todoList).Collection(l => l.SharedWith).IsLoaded)
            {
                await context.Entry(todoList).Collection(l => l.SharedWith).LoadAsync();
            }
        }
        return todoList;
    }

    public async Task<TodoList?> UpdateListAsync(TodoList list, TodoList update)
    {
        list.Title = update.Title;
        await context.SaveChangesAsync();
        logger.LogInformation("Updated TodoList with ID {ListId}", list.Id);
        // Notify users with the fully updated list
        await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ListUpdated", list);
        return list;
    }

    public async Task<GenericOutputDto> DeleteListAsync(TodoList list)
    {
        try
        {
            context.TodoLists.Remove(list);
            await context.SaveChangesAsync();
            logger.LogInformation("Deleted TodoList with ID {ListId}", list.Id);
            return new GenericOutputDto
            {
                Id = list.Id,
                Message = "Todo list deleted successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete TodoList with ID {ListId}", list.Id);
            return new GenericOutputDto
            {
                Id = list.Id,
                Message = "Failed to delete todo list",
                Success = false
            };
        }
    }

    public async Task<GenericOutputDto> AddItemToListAsync(TodoList list, TodoItemForm itemForm)
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
                var mediaDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");

                if (!Directory.Exists(mediaDirectory))
                {
                    Directory.CreateDirectory(mediaDirectory);
                }

                var filePath = Path.Combine(mediaDirectory, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await itemForm.Media.CopyToAsync(stream);
                item.MediaUrl = $"/media/{fileName}";
                item.MediaType = mediaType;
            }

            context.TodoItems.Add(item);
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ItemAdded", item);

            logger.LogInformation("Added item {ItemId} to TodoList {ListId}", item.Id, list.Id);
            return new GenericOutputDto
            {
                Id = item.Id,
                Message = $"Item {item.Id} added successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add item to TodoList with ID {ListId}", list.Id);
            return new GenericOutputDto
            {
                Message = "Failed to add item to todo list",
                Success = false
            };
        }
    }

    public async Task<GenericOutputDto> DeleteItemFromListAsync(TodoList list, TodoItem item)
    {
        try
        {
            context.TodoItems.Remove(item);
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ItemDeleted", item);
            logger.LogInformation("Deleted item {ItemId} from TodoList {ListId}", item.Id, list.Id);
            return new GenericOutputDto
            {
                Id = item.Id,
                Message = $"Item {item.Id} deleted successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete item from TodoList with ID {ListId}", list.Id);
            return new GenericOutputDto
            {
                Message = "Failed to delete item from todo list",
                Success = false
            };
        }
    }

    public async Task<GenericOutputDto> ShareListAsync(TodoList list, ShareRequest request)
    {
        try
        {
            // Check if the user is already shared
            if (list.SharedWith.Any(s => s.SharedWithUserId == request.UserId))
            {
                return new GenericOutputDto
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
            logger.LogInformation("Shared TodoList {ListId} with user {UserId}", list.Id, request.UserId);
            return new GenericOutputDto
            {
                Message = $"List {list.Id} shared with {request.UserId} successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to share TodoList with ID {ListId} with user {UserId}", list.Id, request.UserId);
            return new GenericOutputDto
            {
                Message = $"Failed to share list {list.Id} with {request.UserId}",
                Success = false
            };
        }
    }

    public async Task<GenericOutputDto> UnshareListAsync(TodoList list, TodoListShare share, ShareRequest request)
    {
        try
        {
            context.TodoListShares.Remove(share);
            await context.SaveChangesAsync();
            await hubContext.Clients.User(request.UserId.ToString()).SendAsync("ListUnshared", list);
            logger.LogInformation("Unshared TodoList {ListId} from user {UserId}", list.Id, request.UserId);
            return new GenericOutputDto
            {
                Message = $"List {list.Id} unshared with {request.UserId} successfully",
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unshare TodoList with ID {ListId} from user {UserId}", list.Id, request.UserId);
            return new GenericOutputDto
            {
                Message = $"Failed to unshare list {list.Id} from {request.UserId}",
                Success = false
            };
        }
    }
}