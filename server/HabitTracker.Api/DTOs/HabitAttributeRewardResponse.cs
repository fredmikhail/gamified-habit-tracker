using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class HabitAttributeRewardResponse
{
    public AttributeType AttributeType { get; set; }

    public int XpAmount { get; set; }
}
