using System.Text.Json.Serialization;

namespace Bank.Shared.Enums;

[JsonConverter(typeof(RefundTypeJsonConverter))]
public enum RefundType
{
    Partial,
    Complete
}