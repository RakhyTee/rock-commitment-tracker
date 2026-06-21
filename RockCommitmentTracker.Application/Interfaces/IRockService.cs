using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Interfaces;

public interface IRockService
{ 
     Task<Rock> CreateAsync(string memberId, string title, RockCategory category, DateTime dueDate, string? note);
    Task<IEnumerable<Rock>> GetAllAsync(string memberId, RockStatus? status = null);
    Task<Rock> UpdateStatusAsync(string memberId, string rockId, RockStatus newStatus);
    Task<EnrichedProfile> GetEnrichedProfileAsync(string memberId);
}
