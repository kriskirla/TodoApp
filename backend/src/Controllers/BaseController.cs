using Microsoft.AspNetCore.Mvc;
using TodoApp.Models;

namespace TodoApp.Controllers;

public abstract class BaseController : ControllerBase
{
    protected IActionResult FromServiceResult<T>(ServiceResult<T> result)
    {
        if (result.Data != null)
        {
            return Ok(result.Data);
        }

        return result.Error!.Type switch
        {
            ServiceErrorType.Unauthorized => Unauthorized(result.Error.Message),
            ServiceErrorType.NotFound => NotFound(result.Error.Message),
            ServiceErrorType.Forbidden => Forbid(result.Error.Message),
            ServiceErrorType.BadRequest => BadRequest(result.Error.Message),
            _ => StatusCode(500, "Unknown error")
        };
    }
}