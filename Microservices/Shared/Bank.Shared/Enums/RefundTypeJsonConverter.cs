using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bank.Shared.Enums;

public sealed class RefundTypeJsonConverter
    : JsonConverter<RefundType>
{
    public override RefundType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var code = reader.GetString();

        if (code is null)
            throw new JsonException("Refund type cannot be null.");

        try
        {
            return RefundTypeExtensions.FromDatabaseCode(code);
        }
        catch (ArgumentException ex)
        {
            throw new JsonException("Invalid refund type.", ex);
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        RefundType value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToDatabaseCode());
    }
}