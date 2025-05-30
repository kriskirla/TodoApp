using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApp.Data;
using TodoApp.Enums;
using TodoApp.Models;
using TodoApp.Services;
using TodoApp.Util;

namespace TodoApp.Tests;

public class TodoAppTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IHubContext<TodoHub>> _hubContextMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly Mock<ILogger<TodoService>> _loggerMock;
    private readonly TodoService _todoService;
    private readonly Mock<IUserService> _userService;
    private readonly Mock<IUserContext> _userContext;
    private readonly Guid _testUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");


    public TodoAppTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _hubContextMock = new Mock<IHubContext<TodoHub>>();
        _clientProxyMock = new Mock<IClientProxy>();
        _loggerMock = new Mock<ILogger<TodoService>>();
        _userService = new Mock<IUserService>();

        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        _userContext = new Mock<IUserContext>();
        _userContext.Setup(u => u.UserId).Returns(_testUserId);

        _todoService = new TodoService(_context, _userContext.Object, _hubContextMock.Object, _loggerMock.Object, _userService.Object);
    }

    [Fact]
    public async Task CreateListAsync_ShouldReturnSuccess()
    {
        var list = new TodoList
        {
            Title = "Test List",
            OwnerId = _testUserId
        };

        var result = await _todoService.CreateListAsync(list);

        Assert.NotNull(result.Data);
        Assert.Equal(1, _context.TodoLists.Count());
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnList()
    {
        var listId = Guid.NewGuid();
        await _context.TodoLists.AddAsync(new TodoList
        {
            Id = listId,
            Title = "Sample",
            OwnerId = _testUserId
        });
        await _context.SaveChangesAsync();

        var result = await _todoService.GetListAsync(listId);

        Assert.NotNull(result.Data);
        Assert.Equal(listId, result.Data.Id);
    }

    [Fact]
    public async Task UpdateListAsync_ShouldUpdateAndReturnSuccess()
    {
        var listId = Guid.NewGuid();
        await _context.TodoLists.AddAsync(new TodoList
        {
            Id = listId,
            Title = "Old",
            OwnerId = _testUserId
        });
        await _context.SaveChangesAsync();

        var update = new TodoList
        {
            Title = "New"
        };

        var result = await _todoService.UpdateListAsync(listId, update);

        Assert.NotNull(result.Data);
        Assert.Equal("New", result.Data.Title);
    }

    [Fact]
    public async Task DeleteListAsync_ShouldReturnSuccess()
    {
        var listId = Guid.NewGuid();
        await _context.TodoLists.AddAsync(new TodoList
        {
            Id = listId,
            OwnerId = _testUserId
        });
        await _context.SaveChangesAsync();

        var result = await _todoService.DeleteListAsync(listId);

        Assert.NotNull(result.Data);
        Assert.Empty(_context.TodoLists);
    }

    [Fact]
    public async Task AddItemToListAsync_ShouldReturnSuccess()
    {
        var listId = Guid.NewGuid();
        await _context.TodoLists.AddAsync(new TodoList
        {
            Id = listId,
            OwnerId = _testUserId
        });
        await _context.SaveChangesAsync();

        var itemForm = new TodoItemForm { Description = "New Item" };

        var result = await _todoService.AddItemToListAsync(listId, itemForm);

        Assert.NotNull(result.Data);
        Assert.Equal(1, _context.TodoItems.Count());
    }

    [Fact]
    public async Task DeleteItemFromListAsync_ShouldReturnSuccess()
    {
        var listId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await _context.TodoLists.AddAsync(new TodoList
        {
            Id = listId,
            OwnerId = _testUserId
        });
        await _context.TodoItems.AddAsync(new TodoItem
        {
            Id = itemId,
            TodoListId = listId
        });
        await _context.SaveChangesAsync();

        var result = await _todoService.DeleteItemFromListAsync(listId, itemId);

        Assert.NotNull(result.Data);
        Assert.Empty(_context.TodoItems);
    }

    [Fact]
    public async Task ShareListAsync_ShouldReturnSuccess()
    {
        var listId = Guid.NewGuid();
        var shareWithUserId = Guid.NewGuid();

        await _context.Users.AddAsync(new User { Id = shareWithUserId, Username = "Viewer" });

        await _context.TodoLists.AddAsync(new TodoList
        {
            Id = listId,
            Title = "Shared List",
            OwnerId = _testUserId,
            SharedWith = []
        });
        await _context.SaveChangesAsync();

        // Mock GetUser behaviour
        _userService.Setup(s => s.GetUserByIdAsync(shareWithUserId)).ReturnsAsync(
            ServiceResult<User>.Success(new User
            {
                Id = shareWithUserId,
                Username = "Viewer"
            }));

        var request = new ShareRequest
        {
            UserId = shareWithUserId,
            Permission = PermissionType.View
        };

        var result = await _todoService.ShareListAsync(listId, request);

        Assert.NotNull(result.Data);
        Assert.Equal(1, _context.TodoListShares.Count());
    }

    [Fact]
    public async Task UnshareListAsync_ShouldReturnSuccess()
    {
        var listId = Guid.NewGuid();
        var unshareWithUserId = Guid.NewGuid();

        await _context.Users.AddAsync(new User { Id = unshareWithUserId, Username = "Viewer" });

        await _context.TodoLists.AddAsync(new TodoList
        {
            Id = listId,
            Title = "Shared List",
            OwnerId = _testUserId,
            SharedWith = []
        });

        await _context.TodoListShares.AddAsync(new TodoListShare
        {
            Id = Guid.NewGuid(),
            TodoListId = listId,
            SharedWithUserId = unshareWithUserId
        });
        await _context.SaveChangesAsync();

        _userService.Setup(s => s.GetUserByIdAsync(unshareWithUserId)).ReturnsAsync(
            ServiceResult<User>.Success(new User
            {
                Id = unshareWithUserId,
                Username = "Viewer"
            }));

        var result = await _todoService.UnshareListAsync(listId, unshareWithUserId);

        Assert.NotNull(result.Data);
        Assert.Empty(_context.TodoListShares);
    }

    [Fact]
    public async Task FilterListItems_ShouldReturnFilteredItems()
    {
        var listId = Guid.NewGuid();

        var todoList = new TodoList
        {
            Id = listId,
            Title = "Test List",
            OwnerId = _testUserId,
            Items =
            [
                new TodoItem { Id = Guid.NewGuid(), Name = "Item1", Description = "Testing is great!" },
                new TodoItem { Id = Guid.NewGuid(), Name = "Item2", Description = "Tes" }
            ]
        };

        _context.TodoLists.Add(todoList);
        await _context.SaveChangesAsync();

        var result = await _todoService.FilterListItemsAsync(listId, AttributeType.Description, "Test");

        Assert.True(result.Data != null, result.Error?.Message);
        Assert.Single(result.Data.Items);
        Assert.Equal("Item1", result.Data.Items.First().Name);
    }

    [Fact]
    public async Task SortListItems_ShouldReturnSortedItems()
    {
        var listId = Guid.NewGuid();

        var todoList = new TodoList
        {
            Id = listId,
            Title = "Test List",
            OwnerId = _testUserId,
            Items =
            [
                new TodoItem { Id = Guid.NewGuid(), Name = "Banana" },
                new TodoItem { Id = Guid.NewGuid(), Name = "Apple" },
                new TodoItem { Id = Guid.NewGuid(), Name = "Cherry" }
            ]
        };

        _context.TodoLists.Add(todoList);
        _context.TodoItems.AddRange(todoList.Items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _todoService.SortListItemsAsync(listId, AttributeType.Name, OrderType.Ascending);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Items.Count);

        var sortedNames = result.Data.Items.Select(i => i.Name).ToList();
        var expectedOrder = new List<string> { "Apple", "Banana", "Cherry" };

        Assert.Equal(expectedOrder, sortedNames!);
    }

    [Fact]
    public async Task SortFilteredListItems_ShouldReturnSortedFilteredItems()
    {
        var listId = Guid.NewGuid();

        var todoList = new TodoList
        {
            Id = listId,
            Title = "Test List",
            OwnerId = _testUserId,
            Items =
            [
                new TodoItem { Id = Guid.NewGuid(), Name = "Banana", Priority = PriorityType.Critical },
                new TodoItem { Id = Guid.NewGuid(), Name = "Apple", Priority = PriorityType.High },
                new TodoItem { Id = Guid.NewGuid(), Name = "Cherry", Priority = PriorityType.High }
            ]
        };

        _context.TodoLists.Add(todoList);
        _context.TodoItems.AddRange(todoList.Items);
        await _context.SaveChangesAsync();

        // Act
        var result = await _todoService.SortFilteredListItemsAsync(listId, AttributeType.Priority, "2", AttributeType.Name, OrderType.Ascending);

        // Assert
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count);

        var sortedNames = result.Data.Items.Select(i => i.Name).ToList();
        var expectedResult = new List<string> { "Apple", "Cherry" };

        Assert.Equal(expectedResult, sortedNames!);
    }
}
