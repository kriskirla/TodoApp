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

    public TodoAppTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _hubContextMock = new Mock<IHubContext<TodoHub>>();
        _clientProxyMock = new Mock<IClientProxy>();
        _loggerMock = new Mock<ILogger<TodoService>>();

        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        _todoService = new TodoService(_context, _hubContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateListAsync_ShouldReturnSuccess()
    {
        var list = new TodoList { Title = "Test List" };

        var result = await _todoService.CreateListAsync(list);

        Assert.True(result.Success);
        Assert.Equal("Todo list created successfully", result.Message);
        Assert.Equal(1, _context.TodoLists.Count());
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnList()
    {
        var listId = Guid.NewGuid();
        var list = new TodoList { Id = listId, Title = "Sample" };
        await _context.TodoLists.AddAsync(list);
        await _context.SaveChangesAsync();

        var result = await _todoService.GetListAsync(listId);

        Assert.NotNull(result);
        Assert.Equal(listId, result.Id);
    }

    [Fact]
    public async Task UpdateListAsync_ShouldUpdateAndReturnSuccess()
    {
        var list = new TodoList { Id = Guid.NewGuid(), Title = "Old" };
        await _context.TodoLists.AddAsync(list);
        await _context.SaveChangesAsync();

        var update = new TodoList { Title = "New" };

        var result = await _todoService.UpdateListAsync(list, update);

        Assert.True(result.Success);
        Assert.Equal("New", list.Title);
    }

    [Fact]
    public async Task DeleteListAsync_ShouldReturnSuccess()
    {
        var list = new TodoList { Id = Guid.NewGuid(), Title = "To Delete" };
        await _context.TodoLists.AddAsync(list);
        await _context.SaveChangesAsync();

        var result = await _todoService.DeleteListAsync(list);

        Assert.True(result.Success);
        Assert.Empty(_context.TodoLists);
    }

    [Fact]
    public async Task AddItemToListAsync_ShouldReturnSuccess()
    {
        var list = new TodoList { Id = Guid.NewGuid() };
        await _context.TodoLists.AddAsync(list);
        await _context.SaveChangesAsync();

        var itemForm = new TodoItemForm { Description = "New Item" };

        var result = await _todoService.AddItemToListAsync(list, itemForm);

        Assert.True(result.Success);
        Assert.Contains("added successfully", result.Message);
        Assert.Equal(1, _context.TodoItems.Count());
    }

    [Fact]
    public async Task DeleteItemFromListAsync_ShouldReturnSuccess()
    {
        var list = new TodoList { Id = Guid.NewGuid() };
        var item = new TodoItem { Id = Guid.NewGuid(), TodoListId = list.Id };
        await _context.TodoLists.AddAsync(list);
        await _context.TodoItems.AddAsync(item);
        await _context.SaveChangesAsync();

        var result = await _todoService.DeleteItemFromListAsync(list, item);

        Assert.True(result.Success);
        Assert.Empty(_context.TodoItems);
    }

    [Fact]
    public async Task ShareListAsync_ShouldReturnSuccess()
    {
        var list = new TodoList { Id = Guid.NewGuid(), SharedWith = new List<TodoListShare>() };
        await _context.TodoLists.AddAsync(list);
        await _context.SaveChangesAsync();

        var request = new ShareRequest { UserId = Guid.NewGuid(), Permission = PermissionType.View };

        var result = await _todoService.ShareListAsync(list, request);

        Assert.True(result.Success);
        Assert.Equal(1, _context.TodoListShares.Count());
    }

    [Fact]
    public async Task UnshareListAsync_ShouldReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var list = new TodoList { Id = Guid.NewGuid() };
        var share = new TodoListShare { Id = Guid.NewGuid(), TodoListId = list.Id, SharedWithUserId = userId };

        await _context.TodoLists.AddAsync(list);
        await _context.TodoListShares.AddAsync(share);
        await _context.SaveChangesAsync();

        var request = new ShareRequest { UserId = userId };

        var result = await _todoService.UnshareListAsync(list, share, request);

        Assert.True(result.Success);
        Assert.Empty(_context.TodoListShares);
    }
}
