namespace RockCommitmentTracker.Domain.Exceptions;

public class ValidationException : Exception
{ 
  public IReadOnlyDictionary<string, string[]> Errors { get; }
 
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
 
    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = new[] { error } }) { }
  
}
