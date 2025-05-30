using System.Security.Claims;

namespace TodoApp.Services;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId => Guid.TryParse(
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        out var id) ? id : throw new UnauthorizedAccessException("Invalid user");
}