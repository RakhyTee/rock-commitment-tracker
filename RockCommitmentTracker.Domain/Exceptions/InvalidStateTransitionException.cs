using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Domain.Exceptions;

public class InvalidStateTransitionException : Exception
{
    public RockStatus From { get; }
    public RockStatus To { get; }
 
    public InvalidStateTransitionException(RockStatus from, RockStatus to)
        : base($"Cannot transition from '{from}' to '{to}'. Only pending rocks can be updated.")
    {
        From = from;
        To = to;
    }
}
