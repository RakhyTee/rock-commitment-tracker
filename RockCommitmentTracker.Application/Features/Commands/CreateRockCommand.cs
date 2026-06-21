using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Features.Commands;

public record CreateRockCommand(string MemberId, string Title, RockCategory Category, DateTime DueDate, string? Note);
