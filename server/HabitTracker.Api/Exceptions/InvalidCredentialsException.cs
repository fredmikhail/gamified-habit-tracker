namespace HabitTracker.Api.Exceptions;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("The supplied email or password is incorrect.")
    {
    }
}
