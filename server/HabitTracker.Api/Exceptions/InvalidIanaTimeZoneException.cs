namespace HabitTracker.Api.Exceptions;

public sealed class InvalidIanaTimeZoneException : Exception
{
    public InvalidIanaTimeZoneException()
        : base("The supplied time zone is not a valid IANA identifier.")
    {
    }
}
