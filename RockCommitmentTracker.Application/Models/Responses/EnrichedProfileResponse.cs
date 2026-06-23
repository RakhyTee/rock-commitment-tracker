using RockCommitmentTracker.Application.Models;

namespace RockCommitmentTracker.Application.Models.Responses;

public class EnrichedProfileResponse
{
    public IEnumerable<RockResponse> Rocks { get; set; } = [];
    public UserProfile? Profile { get; set; }
    public bool EnrichmentAvailable { get; set; }
}
