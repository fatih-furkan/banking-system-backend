using Bank.Shared.Enums;

namespace Bank.Shared;

public class ErrorResponse
{
    public ErrorCode ErrorCode { get; }
    public string ErrorDescription { get; }
    public DateTime Timestamp { get; }

    public ErrorResponse(Error? error)
    {
        if (error == null)
        {
            ErrorCode = ErrorCode.UnexpectedErr;
            ErrorDescription = Constants.Constants.ExceptionMessages.UnexpectedError;
        }
        else
        {
            ErrorCode = error.ErrorCode;
            ErrorDescription = error.ErrorDescription;
        }

        Timestamp = DateTime.UtcNow;
    }
    
    public override string ToString()
    {
        return $"ErrorCode: {ErrorCode}\nError Description: {ErrorDescription}\nTime: {Timestamp}";
    }
}