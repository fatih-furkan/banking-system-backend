using System.Text.Json.Serialization;

namespace Bank.Shared.Enums;

[JsonConverter(typeof(ChannelCodeJsonConverter))]
public enum ChannelCode
{
    Branch,
    Fast,
    Pos,
    Online
}