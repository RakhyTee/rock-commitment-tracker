using FluentValidation;
using RockCommitmentTracker.Application.Features.Commands;

namespace RockCommitmentTracker.Application.Validation;

public class CreateRockValidator : AbstractValidator<CreateRockCommand>
{
    public CreateRockValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Title is required.");

        RuleFor(command => command.Category)
            .IsInEnum()
            .WithMessage("Invalid category.");

        RuleFor(command => command.DueDate)
            .GreaterThan(DateTime.Now)
            .WithMessage("Due date must be in the future.");
    }

 }
