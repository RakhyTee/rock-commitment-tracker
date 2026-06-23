using FluentValidation;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Validation;

public class CreateRockValidator : AbstractValidator<CreateRockCommand>
{
    private readonly Dictionary<RockCategory, IRockValidationStrategy> _strategies;

    public CreateRockValidator(IEnumerable<IRockValidationStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Category);

        RuleFor(c => c.MemberId)
            .NotEmpty()
            .WithMessage("Member ID is required.");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Title must not be whitespace.");

        RuleFor(c => c.Category)
            .IsInEnum()
            .WithMessage("Category must be one of: Revenue, Health, Career, Other.");

        RuleFor(c => c.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");

        RuleFor(c => c)
            .Must(ExecuteStrategy)
            .WithMessage(c => GetStrategyErrorMessage(c.Category))
            .WithName("Category");
    }

    private bool ExecuteStrategy(CreateRockCommand command) =>
        !_strategies.TryGetValue(command.Category, out var strategy) || strategy.IsValid(command);

    private string GetStrategyErrorMessage(RockCategory category) =>
        _strategies.TryGetValue(category, out var strategy) ? strategy.ErrorMessage : "Invalid category.";
}
