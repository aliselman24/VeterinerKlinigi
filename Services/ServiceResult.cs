namespace VeterinerKlinigi.Services;

public enum ServiceErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4,
    Unexpected = 5
}

public class ServiceResult
{
    public bool Succeeded { get; protected init; }
    public ServiceErrorType ErrorType { get; protected init; }
    public string? ErrorMessage { get; protected init; }

    public static ServiceResult Ok() => new()
    {
        Succeeded = true,
        ErrorType = ServiceErrorType.None
    };

    public static ServiceResult Fail(ServiceErrorType errorType, string message) => new()
    {
        Succeeded = false,
        ErrorType = errorType,
        ErrorMessage = message
    };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Value { get; private init; }

    public static ServiceResult<T> Ok(T value) => new()
    {
        Succeeded = true,
        ErrorType = ServiceErrorType.None,
        Value = value
    };

    public new static ServiceResult<T> Fail(ServiceErrorType errorType, string message) => new()
    {
        Succeeded = false,
        ErrorType = errorType,
        ErrorMessage = message
    };
}