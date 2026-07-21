using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class UserAttributeResponse
{
    public AttributeType AttributeType { get; set; }

    public int CurrentXp { get; set; }

    public int Level { get; set; }

    public int XpIntoCurrentLevel { get; set; }

    public int XpNeededForNextLevel { get; set; }
}
