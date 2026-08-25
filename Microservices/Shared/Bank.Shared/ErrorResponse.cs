using Bank.Shared.Constants;
using Bank.Shared.Enums;

namespace Bank.Shared;

public class ErrorResponse
{
    public Error Error { get; }
    public DateTime Timestamp { get; }

    public ErrorResponse(Error? error)
    {
        if (error == null)
        {
            this.Error = Errors.UnexpectedError;
        }
        else
        {
            this.Error = error;
        }

        Timestamp = DateTime.Now;
    }
    
    public override string ToString()
    {
        return $"ErrorCode: {Error.ErrorCode}\nError Description: {Error.ErrorDescription}\nTime: {Timestamp}";
    }
}