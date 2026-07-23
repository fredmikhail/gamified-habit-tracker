using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class AttributeLevelUpResponse
{
    public AttributeType AttributeType { get; set; }

    public int CurrentLevel { get; set; }

    public int XpRemaining { get; set; }
}
