using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Models;

public class EnrichedProfile
{
    public IEnumerable<Rock> Rocks { get; set; } = [];
    public UserProfile? Profile { get; set; }
    public bool EnrichmentAvailable { get; set; }
}
