using Google.Apis.Auth.OAuth2;
using IntervalLearningApi.Controllers.Users;
using IntervalLearningApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace IntervalLearningApi.Extensions;

public static class JwtTokenServicesExtensions
{
    public static void AddJwtTokenServices(this IServiceCollection services, 
        IConfiguration configuration,
        ServiceProvider serviceProvider)
    {
        var bindJwtSettings = new JwtSettings();

        configuration.Bind("JsonWebTokenKeys", bindJwtSettings);

        services.Configure<JwtSettings>(configuration.GetSection("JsonWebTokenKeys"));
        services.Configure<GoogleSettings>(configuration.GetSection("GoogleAuth"));
        services.AddSingleton(bindJwtSettings);

        services.AddScoped<IJwtUtils, JwtUtils>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        //services.AddAuthentication(options => 
        //{
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //}).AddJwtBearer(options => {

        //    //options.SecurityTokenValidators.Clear();
        //    //options.SecurityTokenValidators.Add(serviceProvider.GetRequiredService<GoogleTokenValidator>());
        //    //options.SecurityTokenValidators.Add(serviceProvider.GetRequiredService<CustomJwtTokenValidator>());

        //    //options.au

        //    //TODO: not safe
        //    options.RequireHttpsMetadata = false;

        //    options.SaveToken = true;

        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = bindJwtSettings.ValidateIssuerSigningKey,
        //        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(bindJwtSettings.IssuerSigningKey)),
        //        ValidateIssuer = bindJwtSettings.ValidateIssuer,
        //        ValidIssuer = bindJwtSettings.ValidIssuer,
        //        ValidateAudience = bindJwtSettings.ValidateAudience,
        //        ValidAudience = bindJwtSettings.ValidAudience,
        //        RequireExpirationTime = bindJwtSettings.RequireExpirationTime,
        //        ValidateLifetime = bindJwtSettings.RequireExpirationTime,
        //        ClockSkew = TimeSpan.Zero,
        //    };
        //}).AddJwtBearer("Google", o => { });

        //services.AddAuthorization(options =>
        //{
        //    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
        //        JwtBearerDefaults.AuthenticationScheme, "Auth0");
        //    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
        //    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
        //});
    }
}