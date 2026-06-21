namespace RockCommitmentTracker.Domain.Exceptions;

public class NotFoundException : Exception
{
        public NotFoundException(string resourceName, string id)
    : base($"{resourceName} with id '{id}' was not found.") { }
}
