namespace Bank.Shared.Enums;

public static class RefundTypeExtensions
{
    public static string ToDatabaseCode(this RefundType refundType)
    {
        return refundType switch
        {
            RefundType.Partial   => "P",
            RefundType.Complete      => "C",

            _ => throw new ArgumentOutOfRangeException(
                nameof(refundType),
                refundType,
                "Invalid refund type."
            )
        };
    }

    public static RefundType FromDatabaseCode(string code)
    {
        return code switch
        {
            "P" => RefundType.Partial,
            "C" => RefundType.Complete,

            _ => throw new ArgumentException(
                "Invalid refund type",
                nameof(code)
            )
        };
    }
}