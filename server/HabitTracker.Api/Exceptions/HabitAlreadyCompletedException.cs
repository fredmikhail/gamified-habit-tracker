namespace HabitTracker.Api.Exceptions;

public sealed class HabitAlreadyCompletedException : Exception
{
    public HabitAlreadyCompletedException()
        : base("This habit has already been completed for today.")
    {
    }
}
