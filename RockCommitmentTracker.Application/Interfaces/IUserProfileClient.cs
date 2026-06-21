using RockCommitmentTracker.Application.Models;

namespace RockCommitmentTracker.Application.Interfaces;

public interface IUserProfileClient
{ 
     Task<UserProfile?> GetUserAsync(string memberId);
}
