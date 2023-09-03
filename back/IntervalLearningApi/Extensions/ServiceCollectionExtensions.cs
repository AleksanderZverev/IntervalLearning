using DB;
using DB.DependencyInjection;
using IntervalLearningApi.Controllers;
using IntervalLearningApi.Models;
using IntervalLearningApi.Models.Common;
using IntervalLearningApi.Services;
using IntervalLearningApi.Services.Authentication;
using IntervalLearningApi.Services.Dictionary;
using IntervalLearningApi.Services.Jwt;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace IntervalLearningApi.Extensions;

public class SecretConfig
{
    public JwtSettings JwtSettings { get; set; }
}

public static class ServiceCollectionExtensions
{
    public static void AddWeb(this IServiceCollection services, SecretConfig config)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<UserService>();

        services.AddScoped<CollectionService>();
        services.AddScoped<CardsService>();
        services.AddScoped<RepeatsScheduleService>();
        services.AddScoped<ThemeService>();
        services.AddScoped<UserMetadataService>();
        services.AddScoped<DictionaryService>();
        services.AddScoped(typeof(Repository<>));

        services.AddSingleton(config.JwtSettings);

        // services.AddScoped<SessionUser>(provider =>
        // {
        //     var httpContext = provider.GetRequiredService<IHttpContextAccessor>();
        //     return new SessionUser(httpContext.HttpContext.GetUserId());
        // });
        
        var mvcBuilder = services.AddControllers();
        mvcBuilder
            .AddApplicationPart(typeof(CardsController).Assembly)
            .AddControllersAsServices();
        
        mvcBuilder.AddNewtonsoftJson(opts =>
        {
            opts.SerializerSettings.ContractResolver = new DefaultContractResolver()
                {NamingStrategy = new CamelCaseNamingStrategy()};
            opts.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        });

    }
}