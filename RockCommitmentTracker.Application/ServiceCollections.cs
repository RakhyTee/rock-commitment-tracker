using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Application.Models;
using RockCommitmentTracker.Application.Profiles;
using RockCommitmentTracker.Application.Services;
using RockCommitmentTracker.Application.Validation;
using RockCommitmentTracker.Application.Validation.CategoryValidators;

namespace RockCommitmentTracker.Application;

public static class ServiceCollections
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRockService, RockService>();
        services.AddScoped<IValidator<CreateRockCommand>, CreateRockValidator>();

        services.AddScoped<IRockValidationStrategy, RockValidationStrategy>();
        services.AddScoped<IRockValidationStrategy, HealthValidationStrategy>();
        services.AddScoped<IRockValidationStrategy, CareerValidationStrategy>();
        services.AddScoped<IRockValidationStrategy, OtherValidationStrategy>();

        services.AddAutoMapper(cfg => cfg.AddProfile<RockMappingProfile>());

        return services;
    }
}
