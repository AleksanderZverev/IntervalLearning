using System.Reflection;
using DB;
using DomainServices.BoundedContext.Accounts.JwtService;
using FluentValidation;
using GlobalTools;
using Infrastructure.BoundedContexts.Accounts.Jwt;
using IntervalLearningApi.Controllers.Accounts.Requests.Authenticate;
using IntervalLearningApi.Controllers.Study.Cards;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Infrastructure.ValidatorResolver;
using IntervalLearningApi.Services.Dictionary;
using IntervalLearningApi.Services.Statistics;
using Mapster;
using MapsterMapper;
using Newtonsoft.Json.Serialization;

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

        services.AddScoped<CommandManager>();
        services.AddScoped<ValidatorResolver>();
        
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<StatisticsService>();
        services.AddScoped<DictionaryService>();
        services.AddScoped(typeof(Repository<>));

        services.AddSingleton(config.JwtSettings);
        
        services.AddWebMapper();
        services.AddWebValidator();

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
        });
    }

    private static void AddWebMapper(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.RequireExplicitMapping = true;
        config.RequireDestinationMemberSource = true;
        config.AllowImplicitDestinationInheritance = true;

        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

#if DEBUG
        config.Compile();
#endif
    }

    private static void AddWebValidator(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AuthenticateRequestValidator>();
    }
}