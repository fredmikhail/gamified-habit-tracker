using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Domain.Entities;

public sealed class HabitAttributeReward
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid HabitId { get; set; }

    public AttributeType AttributeType { get; set; }

    public int XpAmount { get; set; }

    public Habit Habit { get; set; } = null!;
}