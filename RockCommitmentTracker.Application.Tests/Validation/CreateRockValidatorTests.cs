using FluentAssertions;
using FluentValidation.TestHelper;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Application.Validation;
using RockCommitmentTracker.Application.Validation.CategoryValidators;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Tests.Validation;

public class CreateRockValidatorTests
{
    private readonly CreateRockValidator _validator = new([
        new RockValidationStrategy(),
        new HealthValidationStrategy(),
        new CareerValidationStrategy(),
        new OtherValidationStrategy()
    ]);

    // --- Field rules ---

    [Fact]
    public void Validate_MissingMemberId_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { MemberId = "" });

        result.ShouldHaveValidationErrorFor(c => c.MemberId)
            .WithErrorMessage("Member ID is required.");
    }

    [Fact]
    public void Validate_MissingTitle_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { Title = "" });

        result.ShouldHaveValidationErrorFor(c => c.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_PastDueDate_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with { DueDate = DateTime.UtcNow.AddDays(-1) });

        result.ShouldHaveValidationErrorFor(c => c.DueDate);
    }

    // --- Revenue strategy ---

    [Fact]
    public void Validate_Revenue_DueDateInCurrentQuarter_IsValid()
    {
        var result = _validator.TestValidate(ValidCommand() with
        {
            Category = RockCategory.Revenue,
            DueDate = CurrentQuarterDate()
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Revenue_DueDateOutsideCurrentQuarter_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with
        {
            Category = RockCategory.Revenue,
            DueDate = NextQuarterDate()
        });

        result.ShouldHaveValidationErrorFor("Category")
            .WithErrorMessage("Due date must fall within the current quarter.");
    }

    // --- Health strategy ---

    [Fact]
    public void Validate_Health_TitleAtLeast10Chars_IsValid()
    {
        var result = _validator.TestValidate(ValidCommand() with
        {
            Category = RockCategory.Health,
            Title = "LongEnoughTitle"
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Health_TitleShorterThan10Chars_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with
        {
            Category = RockCategory.Health,
            Title = "Short"
        });

        result.ShouldHaveValidationErrorFor("Category")
            .WithErrorMessage("Title must be at least 10 characters.");
    }

    // --- Career strategy ---

    [Fact]
    public void Validate_Career_WithNote_IsValid()
    {
        var result = _validator.TestValidate(ValidCommand() with
        {
            Category = RockCategory.Career,
            Note = "This matters because it accelerates my growth."
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Career_WithoutNote_HasError()
    {
        var result = _validator.TestValidate(ValidCommand() with
        {
            Category = RockCategory.Career,
            Note = null
        });

        result.ShouldHaveValidationErrorFor("Category")
            .WithErrorMessage("A note field is required explaining why this matters.");
    }

    // --- Other strategy ---

    [Fact]
    public void Validate_Other_NoExtraConstraints_IsValid()
    {
        var result = _validator.TestValidate(ValidCommand() with
        {
            Category = RockCategory.Other,
            Note = null
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    // --- Helpers ---

    private static CreateRockCommand ValidCommand() =>
        new("member-1", "My default title", RockCategory.Other, DateTime.UtcNow.AddDays(7), null);

    private static DateTime CurrentQuarterDate()
    {
        var today = DateTime.Today;
        var quarterStartMonth = (today.Month - 1) / 3 * 3 + 1;
        return new DateTime(today.Year, quarterStartMonth, 1).AddMonths(3).AddDays(-1);
    }

    private static DateTime NextQuarterDate()
    {
        var today = DateTime.Today;
        var quarterStartMonth = (today.Month - 1) / 3 * 3 + 1;
        return new DateTime(today.Year, quarterStartMonth, 1).AddMonths(3);
    }
}
