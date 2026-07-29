using Bank.Shared.Enums;

namespace Bank.Shared;

public record Error(ErrorCode ErrorCode, string ErrorDescription)
{

    public override string ToString()
    {
        return $"Error Code: {ErrorCode}\nError Description: {ErrorDescription}";
    }
}