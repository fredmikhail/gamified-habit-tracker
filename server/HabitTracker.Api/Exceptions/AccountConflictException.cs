namespace HabitTracker.Api.Exceptions;

public sealed class AccountConflictException : Exception
{
    public AccountConflictException()
        : base("An account with the supplied email or username already exists.")
    {
    }
}
