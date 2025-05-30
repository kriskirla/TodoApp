namespace TodoApp.Models;

public class ServiceResult<T>
{
    public T? Data { get; set; }
    public ServiceError? Error { get; set; }

    public static ServiceResult<T> Success(T data) => new() { Data = data };

    public static ServiceResult<T> Unknown(string message) =>
    new() { Error = new ServiceError(ServiceErrorType.NotFound, message) };

    public static ServiceResult<T> Unauthorized(string message) =>
    new() { Error = new ServiceError(ServiceErrorType.NotFound, message) };

    public static ServiceResult<T> NotFound(string message) =>
    new() { Error = new ServiceError(ServiceErrorType.NotFound, message) };

    public static ServiceResult<T> Forbidden(string message) =>
    new() { Error = new ServiceError(ServiceErrorType.Forbidden, message) };

    public static ServiceResult<T> BadRequest(string message) =>
    new() { Error = new ServiceError(ServiceErrorType.BadRequest, message) };
}

public class ServiceError(ServiceErrorType type, string message)
{
    public ServiceErrorType Type { get; set; } = type;
    public string Message { get; set; } = message;
}

public enum ServiceErrorType
{
    Unknown = default,
    Unauthorized,
    BadRequest,
    NotFound,
    Forbidden
}
