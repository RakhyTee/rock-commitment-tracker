using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Interfaces;

public interface IRockValidationStrategy
{
    RockCategory Category { get; }
    string ErrorMessage { get; }
    bool IsValid(CreateRockCommand command);
}
