using IntervalLearningApi.Models;
using IntervalLearningApi.Services.Authentication;
using IntervalLearningApi.Services.Jwt;

namespace IntervalLearningApi.Extensions;

public static class JwtTokenServicesExtensions
{
    public static void AddJwtTokenServices(this IServiceCollection services, IConfiguration configuration)
    {
        var bindJwtSettings = new JwtSettings();
        configuration.Bind("JsonWebTokenKeys", bindJwtSettings);

        services.Configure<JwtSettings>(configuration.GetSection("JsonWebTokenKeys"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleAuth"));
        
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<UserService>();
    }
}