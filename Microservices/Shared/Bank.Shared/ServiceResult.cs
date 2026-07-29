namespace Bank.Shared;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public Error? Error { get; private set; }
    public int StatusCode { get; private set; }

    public static ServiceResult<T> Success(T data, int statusCode = 200)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ServiceResult<T> Failure(Error error, int statusCode = 400)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Error = error,
            StatusCode = statusCode
        };
    }
}