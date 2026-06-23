using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using RockCommitmentTracker.Application.Interfaces;
using RockCommitmentTracker.Infrastructure.ExternalClients;
using RockCommitmentTracker.Infrastructure.Repository;

namespace RockCommitmentTracker.Infrastructure;

public static class ServiceCollections
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRockRepository, RockRepository>();

        services.AddHttpClient<IUserProfileClient, JsonPlaceholderUserProfileClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["JsonPlaceholder:BaseUrl"] ?? "https://jsonplaceholder.typicode.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddResilienceHandler("retry", (pipeline, context) =>
{
    var logger = context.ServiceProvider
        .GetRequiredService<ILogger<JsonPlaceholderUserProfileClient>>();

    var retryOptions = new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = args =>
        {
            logger.LogWarning(
                "Retry attempt {AttemptNumber} of 3. Delay: {DelayMs}ms. Reason: {Reason}",
                args.AttemptNumber + 1,
                Math.Round(args.RetryDelay.TotalMilliseconds),
                args.Outcome.Exception?.Message ?? args.Outcome.Result?.ReasonPhrase);

            return ValueTask.CompletedTask;
        }
    };

    pipeline.AddRetry(retryOptions);
});

        return services;
    }
}
