using IntervalLearningApi.Models;

namespace IntervalLearningApi.Extensions;

public static class JwtTokenServicesExtensions
{
    public static void AddJwtTokenServices(this IServiceCollection services, IConfiguration configuration)
    {
        var bindJwtSettings = new JwtSettings();
        configuration.Bind("JsonWebTokenKeys", bindJwtSettings);

        services.Configure<JwtSettings>(configuration.GetSection("JsonWebTokenKeys"));
    }
}