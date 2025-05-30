using System.Linq.Expressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Enums;
using TodoApp.Models;
using TodoApp.Util;

namespace TodoApp.Services;

public class TodoService(
    AppDbContext context,
    IUserContext userContext,
    IHubContext<TodoHub> hubContext,
    ILogger<TodoService> logger,
    IUserService userService) : ITodoService
{
    // This is a scalable approach applies new filterable/sorable attributes
    // For more, just add within the dictionary
    private static readonly Dictionary<AttributeType, AttributeAccessors> AttributeMap = new()
    {
        [AttributeType.Name] = new AttributeAccessors
        {
            FilterPredicate = (i, v) => i.Name == v.ToString(),
            SortSelector = i => i.Name
        },
        [AttributeType.DueDate] = new AttributeAccessors
        {
            FilterPredicate = (i, v) =>
            {
                if (DateTime.TryParse(v.ToString(), out var parsed))
                {
                    return i.DueDate == null || i.DueDate.Value.ToShortDateString() == parsed.Date.ToShortDateString();
                }
                return false;
            },
            SortSelector = i => i.DueDate
        },
        [AttributeType.Status] = new AttributeAccessors
        {
            FilterPredicate = (i, v) => i.Status.ToString() == v.ToString(),
            SortSelector = i => i.Status
        },
        [AttributeType.Priority] = new AttributeAccessors
        {
            FilterPredicate = (i, v) => i.Priority.ToString() == v.ToString(),
            SortSelector = i => i.Priority
        }
    };

    public async Task<ServiceResult<TodoList>> CreateListAsync(TodoList list)
    {
        try
        {
            list.Id = Guid.NewGuid();
            list.OwnerId = userContext.UserId;
            context.TodoLists.Add(list);
            await context.SaveChangesAsync();
            await hubContext.Clients.User(list.OwnerId.ToString()).SendAsync("ListCreated", list);
            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create todo list {listId}", list.Id);
            return ServiceResult<TodoList>.Unknown("Failed to create todo list");
        }
    }

    public async Task<ServiceResult<TodoList>> GetListAsync(Guid listId)
    {
        try
        {
            return await FetchListAsync(listId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch todo list {listId}", listId);
            return ServiceResult<TodoList>.Unknown("Failed to fetch todolist");;
        }
    }

    public async Task<ServiceResult<TodoList>> UpdateListAsync(Guid listId, TodoList update)
    {
        try
        {
            var result = await FetchListAsync(listId, false, false, false, true);
            var list = result.Data;

            if (list == null)
            {
                return result;
            }

            list.Title = update.Title;
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ListUpdated", list);
            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update todo list {listId}", listId);
            return ServiceResult<TodoList>.Unknown("Failed to update todo list");
        }
    }

    public async Task<ServiceResult<TodoList>> DeleteListAsync(Guid listId)
    {
        try
        {
            var result = await FetchListAsync(listId, false, false, true, false);
            var list = result.Data;

            if (list == null)
            {
                return result;
            }

            // Delete items within the list before deleting list
            var items = context.TodoItems.Where(i => i.TodoListId == list.Id).ToList();

            foreach (var item in items)
            {
                // Ignore items that are not valid
                if (item.Id == null)
                {
                    logger.LogWarning("List {listId} contains item with invalid Guid", list.Id);
                    continue;
                }

                var deletedItem = await DeleteItem(item);

                // Another log to make sure item delete is successful
                if (deletedItem == null)
                {
                    logger.LogWarning("Failed to delete item {ItemId} from list {ListId}", item.Id, list.Id);
                    continue;
                }
            }

            context.TodoLists.Remove(list);
            await context.SaveChangesAsync();
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ListDeleted", list);
            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete todo list {listId}", listId);
            return ServiceResult<TodoList>.Unknown("Failed to delete todo list");
        }
    }

    public async Task<ServiceResult<TodoList>> AddItemToListAsync(Guid listId, TodoItemForm itemForm)
    {
        try
        {
            var result = await FetchListAsync(listId, true, true, false, true);
            var list = result.Data;

            if (list == null)
            {
                return result;
            }

            var item = new TodoItem
            {
                Id = Guid.NewGuid(),
                TodoListId = list.Id,
                Name = itemForm.Name,
                Description = itemForm.Description,
                DueDate = itemForm.DueDate,
                Status = itemForm.Status,
                Priority = itemForm.Priority
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
            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add item to todo list {listId}", listId);
            return ServiceResult<TodoList>.Unknown("Failed to add item to todo list");
        }
    }

    public async Task<ServiceResult<TodoList>> DeleteItemFromListAsync(Guid listId, Guid itemId)
    {
        try
        {
            var result = await FetchListAsync(listId, true, true, false, true);
            var list = result.Data;

            if (list == null)
            {
                return result;
            }

            var item = list.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                return ServiceResult<TodoList>.NotFound("The todo item cannot be found");
            }

            await DeleteItem(item);
            await hubContext.Clients.Group(list.Id.ToString()).SendAsync("ItemDeleted", item);
            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete item from todo list {listId}", listId);
            return ServiceResult<TodoList>.Unknown("Failed to delete item from todo list");
        }
    }

    public async Task<ServiceResult<TodoList>> ShareListAsync(Guid listId, ShareRequest request)
    {
        try
        {
            if (request.UserId == Guid.Empty)
            {
                return ServiceResult<TodoList>.BadRequest("User ID is required");
            }

            var userResult = await userService.GetUserByIdAsync(request.UserId);
            var user = userResult.Data;
            if (user == null)
            {
                return ServiceResult<TodoList>.NotFound(userResult.Error!.Message);
            }

            // The current logic only allows list owner to share list
            // If we decide that shared user with Edit permission can also share,
            // just set requireOwner = false, requireEdit = true
            var result = await FetchListAsync(listId, false, true, true, false);
            var list = result.Data;

            if (list == null)
            {
                return result;
            }

            // Check if the user is already shared
            if (list.SharedWith.Any(s => s.SharedWithUserId == request.UserId))
            {
                return ServiceResult<TodoList>.BadRequest("List is already shared with user");
            }

            context.TodoListShares.Add(new TodoListShare
            {
                Id = Guid.NewGuid(),
                TodoListId = list.Id,
                SharedWithUserId = request.UserId,
                Permission = request.Permission
            });
            await context.SaveChangesAsync();
            await hubContext.Clients.User(request.UserId.ToString()).SendAsync("ListShared", list);
            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to share todo list {listId} with user {userId}", listId, request.UserId);
            return ServiceResult<TodoList>.Unknown($"Failed to share todo list with user");
        }
    }

    public async Task<ServiceResult<TodoList>> UnshareListAsync(Guid listId, Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return ServiceResult<TodoList>.BadRequest("User ID is required");
            }
            var userResult = await userService.GetUserByIdAsync(userId);
            var user = userResult.Data;
            if (user == null)
            {
                return ServiceResult<TodoList>.NotFound(userResult.Error!.Message);
            }

            var result = await FetchListAsync(listId, false, true, true, false);
            var list = result.Data;

            if (list == null)
            {
                return result;
            }

            // Check if user is shared
            var share = list.SharedWith.FirstOrDefault(u => u.SharedWithUserId == user.Id);
            if (share == null)
            {
                return ServiceResult<TodoList>.NotFound("User is not shared with this list");
            }

            context.TodoListShares.Remove(share);
            await context.SaveChangesAsync();
            await hubContext.Clients.User(userId.ToString()).SendAsync("ListUnshared", list);
            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to unshare todo list {listId} from user {userId}", listId, userId);
            return ServiceResult<TodoList>.Unknown("Failed to unshare todo list from user");
        }
    }

    public async Task<ServiceResult<IEnumerable<TodoList>>> GetAllListByUserIdAsync()
    {
        try
        {
            var lists = await context.TodoLists
                .Where(l => l.OwnerId == userContext.UserId || l.SharedWith.Any(s => s.SharedWithUserId == userContext.UserId))
                .ToListAsync();

            if (lists == null)
            {
                return ServiceResult<IEnumerable<TodoList>>.NotFound("No lists found associated with owner");
            }

            return ServiceResult<IEnumerable<TodoList>>.Success(lists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch todo lists for user {UserId}", userContext.UserId);
            return ServiceResult<IEnumerable<TodoList>>.Unknown("Failed to fetch todo lists for user");
        }
    }

    public async Task<ServiceResult<TodoList>> FilterListItems(Guid listId, AttributeType attribute, string key)
    {
        try
        {
            var result = await FetchListAsync(listId, true, false, false, false);
            var list = result.Data;

            if (list == null)
            {
                return result;
            }

            if (AttributeMap.TryGetValue(attribute, out var accessor))
            {
                list.Items = [.. list.Items.Where(i => accessor.FilterPredicate(i, key))];
            }

            return ServiceResult<TodoList>.Success(list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to filter todo list {listId}", listId);
            return ServiceResult<TodoList>.Unknown($"Failed to filter todo list");
        }
    }

    public async Task<ServiceResult<TodoList>> SortListItems(Guid listId, AttributeType attribute, OrderType order)
    {
        var result = await FetchListAsync(listId, true, false, false, false);
        var list = result.Data;

        if (list == null)
        {
            return result;
        }

        // Sorting query for items
        var itemsQuery = context.TodoItems.Where(i => i.TodoListId == listId);

        if (AttributeMap.TryGetValue(attribute, out var accessor))
        {
            itemsQuery = order == OrderType.Descending
                ? itemsQuery.OrderByDescending(accessor.SortSelector)
                : itemsQuery.OrderBy(accessor.SortSelector);
        }

        list.Items = await itemsQuery.ToListAsync();
        return ServiceResult<TodoList>.Success(list);
    }

    #region Private method
    private async Task<ServiceResult<TodoList>> FetchListAsync(
        Guid listId,
        bool loadItem = false,
        bool loadShareWith = false,
        bool requireOwner = false,
        bool requireEdit = false)
    {
        // This checks if eneity is tracked before making a query to the database
        var list = await context.TodoLists.FindAsync(listId);

        if (list == null)
            return ServiceResult<TodoList>.NotFound("The todo list cannot be found");

        // Manually load items/shareWith if required and not already loaded
        if (loadItem && !context.Entry(list).Collection(l => l.Items).IsLoaded)
        {
            await context.Entry(list).Collection(l => l.Items).LoadAsync();
        }
        if (loadShareWith && !context.Entry(list).Collection(l => l.SharedWith).IsLoaded)
        {
            await context.Entry(list).Collection(l => l.SharedWith).LoadAsync();
        }

        // If owner, can do anything
        // If require owner but not, forbidden
        // If require edit but don't have permission, forbidden
        // If list is shared, then can view
        // Otherwise, not owner or not shared, forbidden
        if (IsOwner(list))
        {
            return ServiceResult<TodoList>.Success(list);
        }
        else if (requireOwner && !IsOwner(list))
        {
            return ServiceResult<TodoList>.Forbidden("You are not the owner of this list");
        }
        else if (requireEdit && !IsSharedEditPermission(list))
        {
            return ServiceResult<TodoList>.Forbidden("You lack edit permission to this list");
        }
        else if (IsSharedViewOnly(list) || IsSharedEditPermission(list))
        {
            return ServiceResult<TodoList>.Success(list);
        }
        return ServiceResult<TodoList>.Forbidden("You are not authorized to access this list");
    }

    private async Task<TodoItem> DeleteItem(TodoItem item)
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
        return item;
    }

    private bool IsOwner(TodoList list)
    {
        return list != null && list.OwnerId == userContext.UserId;
    }

    private bool IsSharedViewOnly(TodoList list)
    {
        return list != null
        && list.SharedWith.Any(
            s => s.SharedWithUserId == userContext.UserId
            && s.Permission == PermissionType.View);
    }

    private bool IsSharedEditPermission(TodoList list)
    {
        return list != null
        && list.SharedWith.Any(
            s => s.SharedWithUserId == userContext.UserId
            && s.Permission == PermissionType.Edit);
    }
    #endregion
}