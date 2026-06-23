using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Models;

public record CreateRockCommand(string MemberId, string Title, RockCategory Category, DateTime DueDate, string? Note);
