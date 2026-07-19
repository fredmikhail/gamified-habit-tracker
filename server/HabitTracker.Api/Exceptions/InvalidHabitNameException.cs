namespace HabitTracker.Api.Exceptions;

public sealed class InvalidHabitNameException : Exception
{
    public InvalidHabitNameException()
        : base("Habit name must contain at least one non-whitespace character.")
    {
    }
}