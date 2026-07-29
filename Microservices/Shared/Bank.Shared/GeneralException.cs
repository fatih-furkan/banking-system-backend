using Microsoft.AspNetCore.Http;

namespace Bank.Shared;

public sealed class GeneralException : Exception
{
    public Error Error { get; }

    public int StatusCode { get; }

    public GeneralException(
        Error error,
        int statusCode = StatusCodes.Status500InternalServerError)
        : base(error.ErrorDescription)
    {
        Error = error;
        StatusCode = statusCode;
    }
}