using DB;
using IntervalLearningApi.Services;
using IntervalLearningApi.Services.Authentication;
using IntervalLearningApi.Services.Jwt;

namespace IntervalLearningApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddWeb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<UserService>();

        services.AddScoped<CollectionService>();
        services.AddScoped<CardsService>();
        services.AddScoped<RepeatsScheduleService>();
        services.AddScoped<ThemeService>();
        services.AddScoped<UserMetadataService>();
        services.AddScoped(typeof(Repository<>));
    }
}