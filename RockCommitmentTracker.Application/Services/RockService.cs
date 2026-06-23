using FluentValidation;
using Microsoft.Extensions.Logging;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Services;

public class RockService : IRockService
{
    private readonly IRockRepository _repository;
    private readonly IUserProfileClient _profileClient;
    private readonly IValidator<CreateRockCommand> _validator;
    private readonly ILogger<RockService> _logger;

    public RockService(IRockRepository repository, IUserProfileClient profileClient, IValidator<CreateRockCommand> validator,
        ILogger<RockService> logger)
    {
        _repository = repository;
        _profileClient = profileClient;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Rock> CreateAsync(string memberId, string title, RockCategory category, DateTime dueDate, string? note)
    {
        var command = new CreateRockCommand(memberId, title, category, dueDate, note);

        var result = await _validator.ValidateAsync(command);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var rock = new Rock
        {
            Id = Guid.NewGuid().ToString(),
            MemberId = memberId,
            Title = title,
            Category = category,
            Status = RockStatus.Pending,
            DueDate = dueDate,
            Note = note,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddRockAsync(rock);
         _logger.LogInformation("Rock {RockId} created for member {MemberId}", rock.Id, memberId);

        return rock;
    }

    public async Task<IEnumerable<Rock>> GetAllAsync(string memberId, RockStatus? status = null)
    {
        return await _repository.GetAllRocksAsync(memberId, status);
    }

    public async Task<EnrichedProfile> GetEnrichedProfileAsync(string memberId)
    {
        var rocks = await _repository.GetAllRocksAsync(memberId);

        UserProfile? profile = null;
        var enrichmentAvailable = true;

        try
        {
            profile = await _profileClient.GetUserAsync(memberId);
        }
        catch (Exception ex)
        {
            enrichmentAvailable = false;
            _logger.LogWarning(ex, "Profile enrichment unavailable for member {MemberId}", memberId);
        }

        return new EnrichedProfile
        {
            Rocks = rocks,
            Profile = profile,
            EnrichmentAvailable = enrichmentAvailable
        };
    }

    public async Task<Rock> UpdateStatusAsync(string memberId, string rockId, RockStatus newStatus)
    {
        var rock = await _repository.GetRockByIdAsync(memberId, rockId)
            ?? throw new KeyNotFoundException($"Rock '{rockId}' was not found for member '{memberId}'.");

        if (rock.Status != RockStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot transition from '{rock.Status}' to '{newStatus}'. Only pending rocks can be updated.");

        rock.Status = newStatus;
        await _repository.UpdateAsync(rock);

        _logger.LogInformation("Rock {RockId} status updated to {Status} for member {MemberId}", rockId, newStatus, memberId);

        return rock;
    }
}
