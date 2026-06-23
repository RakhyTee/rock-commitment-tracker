using FluentAssertions;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Application.Validation.CategoryValidators;
using RockCommitmentTracker.Domain.Enums;

namespace RockCommitmentTracker.Application.Tests.Validation;

public class ValidationStrategyTests
{
    // --- Revenue ---

    [Fact]
    public void Revenue_DueDateInCurrentQuarter_IsValid()
    {
        var strategy = new RockValidationStrategy();
        var command = new CreateRockCommand("m1", "Title", RockCategory.Revenue, CurrentQuarterDate(), null);

        strategy.IsValid(command).Should().BeTrue();
    }

    [Fact]
    public void Revenue_DueDateOutsideCurrentQuarter_IsInvalid()
    {
        var strategy = new RockValidationStrategy();
        var command = new CreateRockCommand("m1", "Title", RockCategory.Revenue, NextQuarterDate(), null);

        strategy.IsValid(command).Should().BeFalse();
    }

    [Fact]
    public void Revenue_DueDateInPreviousYear_IsInvalid()
    {
        var strategy = new RockValidationStrategy();
        var command = new CreateRockCommand("m1", "Title", RockCategory.Revenue, new DateTime(DateTime.Today.Year - 1, 6, 30), null);

        strategy.IsValid(command).Should().BeFalse();
    }

    // --- Health ---

    [Fact]
    public void Health_TitleExactly10Chars_IsValid()
    {
        var strategy = new HealthValidationStrategy();
        var command = new CreateRockCommand("m1", "1234567890", RockCategory.Health, DateTime.UtcNow.AddDays(1), null);

        strategy.IsValid(command).Should().BeTrue();
    }

    [Fact]
    public void Health_TitleLongerThan10Chars_IsValid()
    {
        var strategy = new HealthValidationStrategy();
        var command = new CreateRockCommand("m1", "My health goal for the year", RockCategory.Health, DateTime.UtcNow.AddDays(1), null);

        strategy.IsValid(command).Should().BeTrue();
    }

    [Fact]
    public void Health_TitleShorterThan10Chars_IsInvalid()
    {
        var strategy = new HealthValidationStrategy();
        var command = new CreateRockCommand("m1", "Short", RockCategory.Health, DateTime.UtcNow.AddDays(1), null);

        strategy.IsValid(command).Should().BeFalse();
    }

    [Fact]
    public void Health_NullTitle_IsInvalid()
    {
        var strategy = new HealthValidationStrategy();
        var command = new CreateRockCommand("m1", null!, RockCategory.Health, DateTime.UtcNow.AddDays(1), null);

        strategy.IsValid(command).Should().BeFalse();
    }

    // --- Career ---

    [Fact]
    public void Career_WithNote_IsValid()
    {
        var strategy = new CareerValidationStrategy();
        var command = new CreateRockCommand("m1", "Title", RockCategory.Career, DateTime.UtcNow.AddDays(1), "This matters because it grows my skills.");

        strategy.IsValid(command).Should().BeTrue();
    }

    [Fact]
    public void Career_NullNote_IsInvalid()
    {
        var strategy = new CareerValidationStrategy();
        var command = new CreateRockCommand("m1", "Title", RockCategory.Career, DateTime.UtcNow.AddDays(1), null);

        strategy.IsValid(command).Should().BeFalse();
    }

    [Fact]
    public void Career_WhitespaceNote_IsInvalid()
    {
        var strategy = new CareerValidationStrategy();
        var command = new CreateRockCommand("m1", "Title", RockCategory.Career, DateTime.UtcNow.AddDays(1), "   ");

        strategy.IsValid(command).Should().BeFalse();
    }

    // --- Other ---

    [Fact]
    public void Other_Always_IsValid()
    {
        var strategy = new OtherValidationStrategy();
        var command = new CreateRockCommand("m1", "Title", RockCategory.Other, DateTime.UtcNow.AddDays(1), null);

        strategy.IsValid(command).Should().BeTrue();
    }

    // --- Helpers ---

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
