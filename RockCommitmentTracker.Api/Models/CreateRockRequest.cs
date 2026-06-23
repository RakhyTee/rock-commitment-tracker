using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Api.Models;

public class CreateRockRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public RockCategory Category { get; set; }
    public DateTime DueDate { get; set; }
}
