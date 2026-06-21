using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Interfaces;

public interface IRockRepository
{
    Task<IEnumerable<Rock>> GetAllRocksAsync(string memberId, RockStatus? rockStatus = null); 
    Task<Rock?> GetRockByIdAsync(string memberId, string rockId);
    Task<Rock> AddRockAsync(Rock rock);
    Task UpdateAsync(Rock rock);
}



