using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Models;

public class Rock
{
    public string Id { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; } = null!;
    public RockCategory Category { get; set; }
    public RockStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
