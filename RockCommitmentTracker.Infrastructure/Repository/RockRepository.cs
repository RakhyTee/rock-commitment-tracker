using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Infrastructure.Repository;

public class RockRepository : IRockRepository
{
    private readonly ILogger<RockRepository> _logger;
    private readonly ConcurrentDictionary<string, List<Rock>> _rocks = new();

    public RockRepository(ILogger<RockRepository> logger)
    {
        _logger = logger;
    }

    public Task<Rock> AddRockAsync(Rock rock)
    {
      

        var rocks = _rocks.GetOrAdd(rock.MemberId, _ => new List<Rock>());

        lock (rocks)
        {
            rocks.Add(rock);
        }
        _logger.LogInformation("Successfully added rock {RockId} for member {MemberId}", rock.Id, rock.MemberId);

        return Task.FromResult(rock);
    }
 
    public Task<Rock?> GetRockByIdAsync(string memberId, string rockId)
    {

        if(!_rocks.TryGetValue(memberId, out var rocks))
        {
            _logger.LogWarning("No rocks found for member {MemberId}", memberId);
            return Task.FromResult<Rock?>(null);
        }

        lock (rocks)
        {
            var rock = rocks.FirstOrDefault(r => r.Id == rockId);

            if (rock is not null)
                _logger.LogInformation("Successfully retrieved rock {RockId} for member {MemberId}", rockId, memberId);
            else
                _logger.LogWarning("Rock {RockId} not found for member {MemberId}", rockId, memberId);

            return Task.FromResult(rock);
        }

    }
 
    public Task<IEnumerable<Rock>> GetAllRocksAsync(string memberId, RockStatus? status = null)
    {

        if(!_rocks.TryGetValue(memberId, out var rocks))
        {
            _logger.LogWarning("No rocks found for member {MemberId}", memberId);
            return Task.FromResult(Enumerable.Empty<Rock>());
        }

        lock (rocks)
        {
            var result = status.HasValue
                ? rocks.Where(r => r.Status == status.Value)
                : rocks.ToList();

            _logger.LogInformation("Successfully retrieved {RockCount} rocks for member {MemberId}", result.Count(), memberId);

            return Task.FromResult(result);
        }
      
    }
 
    public Task UpdateAsync(Rock rock)
    {
        if(!_rocks.TryGetValue(rock.MemberId, out var rocks))
        {
            _logger.LogWarning("No rocks found for member {MemberId}. Cannot update rock {RockId}", rock.MemberId, rock.Id);
            return Task.CompletedTask;
        }

        lock (rocks)
        {
            var index = rocks.FindIndex(r => r.Id == rock.Id);

            if (index >= 0)
                rocks[index] = rock;
        }
        

        _logger.LogInformation("Successfully updated rock {RockId} for member {MemberId}", rock.Id, rock.MemberId);

        return Task.CompletedTask;
    }

}
