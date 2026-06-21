using RockCommitmentTracker.Domain.Enums;
using RockCommitmentTracker.Domain.Exceptions;

namespace RockCommitmentTracker.Domain.Entities;

public class Rock
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public RockCategory Category { get; private set; }
    public RockStatus Status { get; private set; }
    public string OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Rock()
    {
        Title = null!;
        Description = null!;
        OwnerId = null!;
    }

    public static Rock Create(string title, string description, RockCategory category, string ownerId)
    {
        return new Rock
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Category = category,
            Status = RockStatus.NotStarted,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(RockStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
            throw new InvalidStateTransitionException(Status.ToString(), newStatus.ToString());

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    private static bool IsValidTransition(RockStatus from, RockStatus to) => (from, to) switch
    {
        (RockStatus.NotStarted, RockStatus.InProgress) => true,
        (RockStatus.InProgress, RockStatus.Completed) => true,
        (RockStatus.InProgress, RockStatus.Abandoned) => true,
        _ => false
    };
}
