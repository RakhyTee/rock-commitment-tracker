using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Api.Models;

public class UpdateRockStatusRequest
{
    public RockStatus NewStatus { get; set; }
}
