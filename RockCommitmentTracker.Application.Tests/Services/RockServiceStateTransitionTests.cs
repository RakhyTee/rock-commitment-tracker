using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Application.Services;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Tests.Services;

public class RockServiceStateTransitionTests
{
    private readonly IRockRepository _repository = Substitute.For<IRockRepository>();
    private readonly IUserProfileClient _profileClient = Substitute.For<IUserProfileClient>();
    private readonly IValidator<CreateRockCommand> _validator = Substitute.For<IValidator<CreateRockCommand>>();
    private readonly RockService _service;

    public RockServiceStateTransitionTests()
    {
        _service = new RockService(
            _repository,
            _profileClient,
            _validator,
            NullLogger<RockService>.Instance);
    }

    // --- State transitions ---

    [Theory]
    [InlineData(RockStatus.Completed)]
    [InlineData(RockStatus.Missed)]
    public async Task UpdateStatusAsync_PendingRock_TransitionsToNewStatus(RockStatus newStatus)
    {
        var rock = PendingRock();
        _repository.GetRockByIdAsync("m1", "r1").Returns(rock);

        var result = await _service.UpdateStatusAsync("m1", "r1", newStatus);

        result.Status.Should().Be(newStatus);
        await _repository.Received(1).UpdateAsync(Arg.Is<Rock>(r => r.Status == newStatus));
    }

    [Theory]
    [InlineData(RockStatus.Completed)]
    [InlineData(RockStatus.Missed)]
    public async Task UpdateStatusAsync_NonPendingRock_ThrowsInvalidOperationException(RockStatus existingStatus)
    {
        var rock = PendingRock();
        rock.Status = existingStatus;
        _repository.GetRockByIdAsync("m1", "r1").Returns(rock);

        var act = () => _service.UpdateStatusAsync("m1", "r1", RockStatus.Completed);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only pending rocks can be updated*");
    }

    [Fact]
    public async Task UpdateStatusAsync_NonPendingRock_RepositoryUpdateNotCalled()
    {
        var rock = PendingRock();
        rock.Status = RockStatus.Completed;
        _repository.GetRockByIdAsync("m1", "r1").Returns(rock);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateStatusAsync("m1", "r1", RockStatus.Missed));

        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Rock>());
    }

    [Fact]
    public async Task UpdateStatusAsync_RockNotFound_ThrowsKeyNotFoundException()
    {
        _repository.GetRockByIdAsync("m1", "missing").Returns((Rock?)null);

        var act = () => _service.UpdateStatusAsync("m1", "missing", RockStatus.Completed);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*missing*");
    }

    // --- Create ---

    [Fact]
    public async Task CreateAsync_ValidationFails_ThrowsValidationException()
    {
        var failures = new[] { new ValidationFailure("Title", "Title is required.") };
        _validator.ValidateAsync(Arg.Any<CreateRockCommand>(), default)
            .Returns(new ValidationResult(failures));

        var act = () => _service.CreateAsync("m1", "", RockCategory.Other, DateTime.UtcNow.AddDays(1), null);

        await act.Should().ThrowAsync<ValidationException>();
        await _repository.DidNotReceive().AddRockAsync(Arg.Any<Rock>());
    }

    [Fact]
    public async Task CreateAsync_ValidationPasses_ReturnsRockWithPendingStatus()
    {
        _validator.ValidateAsync(Arg.Any<CreateRockCommand>(), default)
            .Returns(new ValidationResult());
        _repository.AddRockAsync(Arg.Any<Rock>()).Returns(x => x.Arg<Rock>());

        var result = await _service.CreateAsync("m1", "My Rock Title", RockCategory.Other, DateTime.UtcNow.AddDays(1), null);

        result.Status.Should().Be(RockStatus.Pending);
        result.MemberId.Should().Be("m1");
        result.Id.Should().NotBeNullOrEmpty();
    }

    // --- Helper ---

    private static Rock PendingRock() => new()
    {
        Id = "r1",
        MemberId = "m1",
        Status = RockStatus.Pending,
        Title = "Test Rock",
        Category = RockCategory.Other,
        DueDate = DateTime.UtcNow.AddDays(7)
    };
}
