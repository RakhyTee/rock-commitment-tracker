using RockCommitmentTracker.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;
using RockCommitmentTracker.Application.Features.Commands;
using RockCommitmentTracker.Application.Validation;
using FluentValidation;
using FluentValidation.Results;
using RockCommitmentTracker.Domain.Exceptions;

namespace RockCommitmentTracker.Application.Services;

public class RockService : IRockService
{
    private readonly IRockRepository _repository;
    private readonly IUserProfileClient _profileClient;
    //private readonly IEnumerable<IRockValidationStrategy> _validationStrategies;
    //private readonly CreateRockValidator _validator;
    private readonly ILogger<RockService> _logger;

    public RockService(IRockRepository rockRepository,
    IUserProfileClient userProfileClient,
    ILogger<RockService> logger)
    {
        _repository = rockRepository;
        _profileClient = userProfileClient;
        _logger = logger;
    }

    public async Task<Rock> CreateAsync(string memberId, string title, RockCategory category, DateTime dueDate, string? note)
    {
        var command = new CreateRockCommand(memberId, title, category, dueDate, note);

        ValidationResult result = new();//await _validator.ValidateAsync(command);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            throw new Exception($"Validation failed: {string.Join(", ", errors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"))}");
        }

        //var strategy = _validationStrategies.FirstOrDefault(s => s.Category == category);
        //if (strategy is not null)
        // await strategy.ValidateAsync(command);

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

        _logger.LogInformation("Rock created for member {MemberId} with ID {RockId}", memberId, rock.Id);

        return MapToModel(rock);
    }

    public async Task<IEnumerable<Rock>> GetAllAsync(string memberId, RockStatus? status = null)
    {
        var rocks = await _repository.GetAllRocksAsync(memberId, status);
        return rocks.Select(MapToModel);
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
            Rocks = rocks.Select(MapToModel),
            Profile = profile,
            EnrichmentAvailable = enrichmentAvailable
        };
    }

    public async Task<Rock> UpdateStatusAsync(string memberId, string rockId, RockStatus newStatus)
    {
        var rock = await _repository.GetRockByIdAsync(memberId, rockId)
            ?? throw new NotImplementedException($"Rock with ID {rockId} not found for member {memberId}");

        if (rock.Status != RockStatus.Pending)
            throw new InvalidStateTransitionException(rock.Status, newStatus);

        rock.Status = newStatus;

        await _repository.UpdateAsync(rock);

        _logger.LogInformation("Rock {RockId} status updated to {Status} for member {MemberId}", rockId, newStatus, memberId);

        return MapToModel(rock);
    }
    
    private static Rock MapToModel(Rock rock) => new()
    {
        Id = rock.Id,
        MemberId = rock.MemberId,
        Title = rock.Title,
        Category = rock.Category,
        Status = rock.Status,
        DueDate = rock.DueDate,
        Note = rock.Note,
        CreatedAt = rock.CreatedAt
    };
}
