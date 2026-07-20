namespace HabitTracker.Api.Exceptions;

public sealed class InactiveHabitException : Exception
{
    public InactiveHabitException()
        : base("Inactive habits cannot be completed.")
    {
    }
}
