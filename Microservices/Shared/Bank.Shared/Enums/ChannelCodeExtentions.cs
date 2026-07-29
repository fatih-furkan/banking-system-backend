namespace Bank.Shared.Enums;

public static class ChannelCodeExtentions
{
    public static string ToDatabaseCode(this ChannelCode channel)
    {
        return channel switch
        {
            ChannelCode.Branch   => "BRN",
            ChannelCode.Fast      => "FAS",
            ChannelCode.Pos   => "POS",
            ChannelCode.Online => "ONL",

            _ => throw new ArgumentOutOfRangeException(
                nameof(channel),
                channel,
                "Invalid channel code."
            )
        };
    }

    public static ChannelCode FromDatabaseCode(string code)
    {
        return code switch
        {
            "BRN" => ChannelCode.Branch,
            "FAS" => ChannelCode.Fast,
            "POS" => ChannelCode.Pos,
            "ONL" => ChannelCode.Online,

            _ => throw new ArgumentException(
                "Invalid channel code.",
                nameof(code)
            )
        };
    }
}