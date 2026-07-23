using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class XpTransactionResponse
{
    public Guid Id { get; set; }

    public string HabitName { get; set; } = string.Empty;

    public AttributeType AttributeType { get; set; }

    public int Amount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
