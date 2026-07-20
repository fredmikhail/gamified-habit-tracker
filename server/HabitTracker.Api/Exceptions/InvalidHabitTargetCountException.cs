namespace HabitTracker.Api.Exceptions;

public sealed class InvalidHabitTargetCountException : Exception
{
    public InvalidHabitTargetCountException()
        : base(
            "Daily habits must have a target count of 1. "
            + "Weekly habits must have a target count between 1 and 7.")
    {
    }
}
