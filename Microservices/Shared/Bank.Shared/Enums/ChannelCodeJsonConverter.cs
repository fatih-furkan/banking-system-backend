namespace Bank.Shared.Enums;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ChannelCodeJsonConverter
    : JsonConverter<ChannelCode>
{
    public override ChannelCode Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var code = reader.GetString();

        if (code is null)
            throw new JsonException("Channel code cannot be null.");

        try
        {
            return ChannelCodeExtentions.FromDatabaseCode(code);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Invalid channel code.", ex);
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        ChannelCode value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToDatabaseCode());
    }
}