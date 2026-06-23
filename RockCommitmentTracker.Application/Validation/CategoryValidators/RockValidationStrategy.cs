using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Validation.CategoryValidators;

public class RockValidationStrategy : IRockValidationStrategy
{
    public RockCategory Category => RockCategory.Revenue;
    public string ErrorMessage => "Due date must fall within the current quarter.";

    public bool IsValid(CreateRockCommand command)
    {
        var today = DateTime.Today;
        var currentQuarter = (today.Month - 1) / 3 + 1;
        var targetQuarter = (command.DueDate.Month - 1) / 3 + 1;

        return command.DueDate.Year == today.Year && targetQuarter == currentQuarter;
    }
}

public class CareerValidationStrategy : IRockValidationStrategy
{
    public RockCategory Category => RockCategory.Career;
    public string ErrorMessage => "A note field is required explaining why this matters.";

    public bool IsValid(CreateRockCommand command) => !string.IsNullOrWhiteSpace(command.Note);
}

public class HealthValidationStrategy : IRockValidationStrategy
{
    public RockCategory Category => RockCategory.Health;
    public string ErrorMessage => "Title must be at least 10 characters.";

    public bool IsValid(CreateRockCommand command) => command.Title?.Length >= 10;
}


public class OtherValidationStrategy : IRockValidationStrategy
{
    public RockCategory Category => RockCategory.Other;
    public string ErrorMessage => string.Empty;

    public bool IsValid(CreateRockCommand command) => true;
}


